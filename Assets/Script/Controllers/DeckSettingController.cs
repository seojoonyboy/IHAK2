using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using DataModules;
using System;
using UniRx;
using UniRx.Triggers;
using Spine.Unity;


public class DeckSettingController : Singleton<DeckSettingController> {
    protected DeckSettingController() { }
    GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.DeckSettingScene;
    GameSceneManager gsm;

    AccountManager playerInfosManager;
    ConstructManager constructManager;

    Camera cam;
    [Header(" - UI")]
    [SerializeField] private GameObject togglePref;
    [SerializeField] private Button chooseSpeciesBtn;
    [SerializeField] private Button speciesConfirmBtn;
    [SerializeField] public Button deleteButton;
    [SerializeField] private GameObject modal;
    [SerializeField] public GameObject cardsContent;
    [SerializeField] public GameObject activeSlotUI;
    [SerializeField] public Button resetButton;
    [SerializeField] public GameObject prodDetailModal;
    [SerializeField] public GameObject unitGenDetailModal;

    public Text
        modalHeader,
        content;

    [SerializeField] Sprite[] speciesPortraits;

    [Header(" - TileGroup")]
    [SerializeField] public GameObject tileGroup;
    [SerializeField] public List<int> tileSetList;
    [SerializeField] public int tileCount = 0;

    [Header(" - EditingBuilding")]
    [SerializeField] public GameObject selectBuilding;
    [SerializeField] public GameObject saveSelectBuilding;
    [SerializeField] public GameObject targetTile;
    [SerializeField] public Vector3 startEditPosition;
    [SerializeField] public bool picking = false;
    [SerializeField] public int startSortingOrder = 0;
    [SerializeField] public GameObject swapTargetBuilding;

    [Header(" - Flag")]
    [SerializeField] public bool reset = false;
    [SerializeField] public bool modify;
    public bool saveBtnClick = false;
    public static Deck prevData = null;

    [Header(" - UISlider")]
    public Slider[] sliders;


    [Header(" - UserData")]
    private int speciesId = 0;
    private int deckCount;

    [Header(" - Time")]
    public float clicktime = 0f;
    public float requireClickTime = 0.3f;
    
    public int SpeciesId {
        get {
            return speciesId;
        }
    }
    
    

    private void Start() {
        playerInfosManager = AccountManager.Instance;
        constructManager = ConstructManager.Instance;
        cardsContent = transform.GetChild(0).GetChild(1).gameObject; // Canvas => UnitScrollPanel => Content;
        activeSlotUI = transform.GetChild(4).GetChild(0).gameObject; // Canvas => ActiveEffectPanel => Content;
        deckCount = playerInfosManager.decks.Count;
        gsm = FindObjectOfType<GameSceneManager>();
        cam = Camera.main;
        var downStream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));
        var dragStream = Observable.EveryUpdate().Where(_=>Input.GetMouseButton(0));
        var upStream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonUp(0));
        playerInfosManager.transform.GetChild(0).GetChild(playerInfosManager.selectNumber).gameObject.SetActive(true);
        tileGroup = playerInfosManager.transform.gameObject.transform.GetChild(0).GetChild(playerInfosManager.selectNumber).gameObject;
        tileCount = tileGroup.transform.childCount - 1;
        TilebuildingList();
        CheckCardCount();
        ResetActiveSlot();
        DeckActiveCheck();
        resetButton.OnClickAsObservable().Subscribe(_ => resetTile());
        deleteButton.OnClickAsObservable().Subscribe(_ => DeleteBuilding());
        

        downStream.Subscribe(_ => PickEditBuilding());
        dragStream.Where(_ => (clicktime < requireClickTime) && (picking== true || selectBuilding != null)).Subscribe(_ => clicktime += Time.deltaTime);
        dragStream.Where(_=>clicktime >= requireClickTime).Subscribe(_ => MoveEditBuilding());
        upStream.Where(_ => clicktime < requireClickTime && selectBuilding != null).Subscribe(_=> ShowDetail(selectBuilding.GetComponent<BuildingObject>()));
        upStream.Where(_=> clicktime >= requireClickTime).Subscribe(_ => DropEditBuilding());
        upStream.Subscribe(_ => clicktime = 0f);
        
        chooseSpeciesBtn.onClick
            .AsObservable()
            .Subscribe(_ => {
                modal.SetActive(true);
            });
        speciesConfirmBtn.onClick
            .AsObservable()
            .Subscribe(_ => {
                modal.SetActive(false);
            });
        modal.GetComponent<Button>().onClick.AsObservable().Subscribe(_ => {
            Modal.instantiate("종족 선택을 취소하시겠습니까?", Modal.Type.YESNO, () => modal.SetActive(false));
        });

        modal.transform.Find("InnerModal/Header/ExitBtn").GetComponent<Button>().onClick.AsObservable().Subscribe(_ => {
            Modal.instantiate("종족 선택을 취소하시겠습니까?", Modal.Type.YESNO, () => modal.SetActive(false));
        });

        ResetAllSliderValues();



        if (prevData != null) {
            int[] coords = prevData.coordsSerial;
            Cost product = null;
            foreach (int coord in coords) {
                GameObject obj = constructManager.GetBuildingObjectById(coord);
                if (obj != null) {
                    BuildingObject buildingObject = obj.GetComponent<BuildingObject>();
                    product = buildingObject.data.card.product;
                    if (product != null) {
                        ChangeSliderValue(product);
                    }
                }
            }
        }
        else
            deckCount++;
    }

    /// <summary>
    /// 준비중인 종족 Toggle 메뉴에 표시
    /// </summary>
    /// <param name="num"></param>
    private void AddPrepareToggle(int num) {
        ToggleGroup toggleGroup = modal.transform.Find("InnerModal/Body/DataArea/ToggleGroup").GetComponent<ToggleGroup>();
        GameObject pref = Instantiate(new GameObject(), toggleGroup.transform);
        Image image = pref.AddComponent<Image>();
        image.sprite = speciesPortraits[0];
        image.color = new Color(0, 0, 0, 1);
        pref.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 160);
        pref.AddComponent<Button>().onClick.AddListener(() => Modal.instantiate("준비중입니다.", Modal.Type.CHECK));
    }

    public void OnPrepareModal() {
        Modal.instantiate("준비중입니다.", Modal.Type.CHECK);
    }

    public void Toggle(Toggle toggle) {
        toggle.transform.Find("Active").gameObject.SetActive(toggle.isOn);
        toggle.transform.Find("Deactive").gameObject.SetActive(!toggle.isOn);
    }

    private void SetModalText(int id) {
        //string species = ((Species.Type)id).ToString();
        //modalHeader.text = species;
        //content.text = species + "에 대한 설명";
    }

    public void SettingButton() {

        if (SetAllTileBuildingCheck() == true) {
            if (prevData == null) {
                saveBtnClick = true;
                GameObject modal = Modal.instantiateWithClose("덱 이름 설정", "덱 이름을 입력해주세요", null, Modal.Type.INSERT, OnclickInputConfirm);
                modal.transform.Find("ModalWindow/Modal/Top/Insert/InputField").GetComponent<InputField>().characterLimit = 16;
                modal.AddComponent<Button>().onClick.AddListener(() => Destroy(modal));
            }
            else {
                saveBtnClick = true;
                string name = prevData.name;
                string str = name;
                if (name.Length > 8) {
                    str = name.Substring(0, 8);
                }
                GameObject modal = Modal.instantiateWithClose("덱 이름 설정", null, str, Modal.Type.INSERT, OnclickInputConfirm);
                modal.transform.Find("ModalWindow/Modal/Top/Insert/InputField").GetComponent<InputField>().characterLimit = 16;
                modal.AddComponent<Button>().onClick.AddListener(() => Destroy(modal));
            }
        }
        else
            Modal.instantiate("갈색 타일내에 건물 배치를 완료해야합니다.", Modal.Type.CHECK);
    }

    private void OnclickInputConfirm(string inputText) {
        Deck deck = new Deck();
        deck.race = "primal";
        deck.name = inputText;
        deck.coordsSerial = new int[tileSetList.Count + 1];
        for (int i = 0; i < tileSetList.Count; i++) {
            deck.coordsSerial[i] = tileSetList[i];
            if (tileGroup.transform.GetChild(i).childCount != 0)
                tileGroup.transform.GetChild(i).GetChild(0).GetComponent<BuildingObject>().setTileLocation = tileGroup.transform.GetChild(i).GetComponent<TileObject>().tileNum;
        }
        deck.coordsSerial = tileSetList.ToArray();
        if (prevData == null) {
            playerInfosManager.AddDeck(deck);
        }
        else {
            deck.isRepresent = prevData.isRepresent;
            deck.id = prevData.id;
            playerInfosManager.decks[playerInfosManager.selectNumber] = deck;
            playerInfosManager.ModifyDeck(deck);
        }
        prevData = null;

        tileGroup.SetActive(false);
        gsm.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
    }

    public void ReturnBtn() {
        Modal.instantiate("배치된 건물 데이터가 저장되지 않습니다.\n정말 나가시겠습니까?", Modal.Type.YESNO, () => {
            Return();
        });
    }

    private void Return() {
        prevData = null;
        
        if (reset == false) {
            for (int i = 0; i < tileCount; i++) {
                if (playerInfosManager.selectNumber > playerInfosManager.decks.Count - 1) {
                    if (tileGroup.transform.GetChild(i).childCount != 0) {
                        Destroy(tileGroup.transform.GetChild(i).GetChild(0).gameObject);
                        tileGroup.transform.GetChild(i).GetComponent<TileObject>().buildingSet = false;
                        continue;
                    }
                }
                else if (playerInfosManager.decks[playerInfosManager.selectNumber] != null) {
                    if (tileGroup.transform.GetChild(i).childCount != 0) {
                        GameObject building = tileGroup.transform.GetChild(i).GetChild(0).gameObject;

                        if (building.GetComponent<BuildingObject>().setTileLocation < 0) {
                            if (building.transform.parent.childCount == 1)
                                building.transform.parent.GetComponent<TileObject>().buildingSet = false;

                            Destroy(building);
                            continue;
                        }

                        if (building.GetComponent<BuildingObject>().setTileLocation != tileGroup.transform.GetChild(i).GetComponent<TileObject>().tileNum) {
                            building.transform.parent.GetComponent<TileObject>().buildingSet = false;
                            building.transform.SetParent(tileGroup.transform.GetChild(building.GetComponent<BuildingObject>().setTileLocation));
                            building.transform.position = building.transform.parent.position;
                            building.transform.parent.GetComponent<TileObject>().buildingSet = true;
                        }
                    }
                }
            }
        }
        else if (reset == true) {
            if (playerInfosManager.selectNumber > playerInfosManager.decks.Count - 1) {
                for (int i = 0; i < tileGroup.transform.childCount; i++) {
                    if (tileGroup.transform.GetChild(i).childCount != 0)
                        Destroy(tileGroup.transform.GetChild(i).GetChild(0).gameObject);
                }
            }
                //playerInfosManager.SetTileObjects(playerInfosManager.selectNumber);
        }
        
        // playerInfosManager.SetTileObjects(playerInfosManager.selectNumber);

        /*
        for(int i = 0; i< tileGroup.transform.childCount; i++) {
            if (tileGroup.transform.GetChild(i).childCount != 0)
                Destroy(tileGroup.transform.GetChild(i).GetChild(0).gameObject);
        }
        */
        if (playerInfosManager.selectNumber > deckCount - 1)
            return;


        tileGroup.SetActive(false);
        //playerInfosManager.checkDeck(playerInfosManager.selectNumber);
        gsm.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
    }

    public void TilebuildingList() {
        for (int i = 0; i < tileCount; i++) {
            if (tileGroup.transform.GetChild(i).childCount != 0)
                tileSetList.Add(tileGroup.transform.GetChild(i).GetChild(0).GetComponent<BuildingObject>().data.id);
            else
                tileSetList.Add(0);
        }
    }

    public void resetTile() {
        for (int i = 0; i < tileCount; i++) {
            if (i == tileCount / 2)
                continue;
            if (tileGroup.transform.GetChild(i).childCount != 0) {
                /*
                GameObject card = FindCard(tileGroup.transform.GetChild(i).GetChild(0).GetComponent<BuildingObject>().data.id);
                int count = BuildingCount(tileGroup.transform.GetChild(i).GetChild(0).gameObject);
                --count;
                card.transform.GetChild(2).GetComponent<Text>().text = count.ToString() + " / " + tileGroup.transform.GetChild(i).GetChild(0).GetComponent<BuildingObject>().data.card.placementLimit;
                */
                Destroy(tileGroup.transform.GetChild(i).GetChild(0).gameObject); // accountManager => 하위 => tileGroup(몇번째 그룹?) => GetChild(i) i번째 타일 => GetChild(0) 0번째에 있는 건물
            }


            tileGroup.transform.GetChild(i).GetComponent<TileObject>().buildingSet = false;
            tileSetList[i] = 0;
        }
        ResetAllSliderValues();
        ResetCardCount();
        ResetActiveSlot();
        reset = true;
    }

    public void PickEditBuilding() {
        if (saveBtnClick == true)
            return;
        if (cam == null)
            return;

        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null) {

            if (hit.collider.tag == "Building") {
                selectBuilding = hit.transform.gameObject;
                saveSelectBuilding = selectBuilding;
                startSortingOrder = GetSortingOrder(selectBuilding);
                if (selectBuilding.GetComponent<BuildingObject>().data.id == -1)
                    selectBuilding = null;

            }
            else if (hit.collider.tag == "Tile") {
                if (hit.transform.gameObject.transform.childCount != 0) {
                    selectBuilding = hit.transform.GetChild(0).gameObject;
                    saveSelectBuilding = selectBuilding;
                    startSortingOrder = GetSortingOrder(selectBuilding);
                    if (selectBuilding.GetComponent<BuildingObject>().data.id == -1)
                        selectBuilding = null;
                }
                else {
                    gameObject.transform.GetChild(3).gameObject.SetActive(false);
                    return;
                }
            }
            else if (hit.collider.tag == "BackGroundTile") {
                gameObject.transform.GetChild(3).gameObject.SetActive(false);
                return;
            }

            if (selectBuilding != null) {
                selectBuilding.GetComponent<PolygonCollider2D>().enabled = false;
                startEditPosition = selectBuilding.transform.position;
            } 
        }
        /*
        else
            gameObject.transform.GetChild(4).gameObject.SetActive(false);
            */
        
    }

    public void MoveEditBuilding() {
        if (saveBtnClick == true)
            return;
        if (selectBuilding == null)
            return;

        picking = true;
        //cam.GetComponent<BitBenderGames.MobileTouchCamera>().enabled = false;        
        Vector3 mousePosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
        mousePosition.z = 0;

        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        

        if (hit.collider != null) {
            if (hit.collider.tag == "Tile") {
                if (hit.collider.GetComponent<TileObject>().Tier <= playerInfosManager.userTier) {
                    targetTile = hit.transform.gameObject;
                    Vector3 buildingPosition = targetTile.transform.position;
                    buildingPosition.z = 0;
                    selectBuilding.transform.position = buildingPosition;
                    SetSortingOrder(selectBuilding, tileCount * 2 - targetTile.GetComponent<TileObject>().tileNum + 1);
                    if ((targetTile.GetComponent<TileObject>().buildingSet == false || selectBuilding.transform.parent.gameObject == targetTile) && playerInfosManager.userTier >= targetTile.GetComponent<TileObject>().Tier) {
                        DragSwap(null);
                        SetColor(selectBuilding, Color.green);
                    }
                    else if (targetTile.GetComponent<TileObject>().buildingSet == true || playerInfosManager.userTier < targetTile.GetComponent<TileObject>().Tier) {
                        DragSwap(targetTile.transform.GetChild(0).gameObject);
                        SetColor(selectBuilding, Color.red);
                    }
                }
                else {
                    targetTile = null;
                    DragSwap(null);
                    SetColor(selectBuilding, Color.white);
                    selectBuilding.transform.position = mousePosition;
                }
            }
            else if (hit.collider.tag == "Building") {
                DragSwap(hit.collider.gameObject);
                targetTile = hit.transform.parent.gameObject;
                Vector3 buildingPosition = targetTile.transform.position;
                buildingPosition.z = 0;
                selectBuilding.transform.position = buildingPosition;
                SetSortingOrder(selectBuilding, 60);
                SetColor(selectBuilding, Color.red);
            }
            else if(hit.collider.tag == "BackGroundTile") {
                targetTile = null;
                DragSwap(null);
                SetColor(selectBuilding, Color.white);
                selectBuilding.transform.position = mousePosition;
            }

        }
        else {
            targetTile = null;
            SetColor(selectBuilding, Color.white);
            selectBuilding.transform.position = mousePosition;
        }
    }

    public void DropEditBuilding() {
        if (saveBtnClick == true)
            return;
        if (selectBuilding == null)
            return;
        if(targetTile != null) {
            if (playerInfosManager.userTier >= targetTile.GetComponent<TileObject>().Tier) {
                if (targetTile.GetComponent<TileObject>().buildingSet == false) {
                    Vector3 position = targetTile.transform.position;
                    position.z = 0;
                    tileSetList[targetTile.GetComponent<TileObject>().tileNum] = tileSetList[selectBuilding.transform.parent.GetComponent<TileObject>().tileNum];
                    selectBuilding.transform.parent.GetComponent<TileObject>().buildingSet = false;
                    tileSetList[selectBuilding.transform.parent.GetComponent<TileObject>().tileNum] = 0;
                    selectBuilding.transform.SetParent(targetTile.transform);
                    selectBuilding.transform.position = position;
                    SetSortingOrder(selectBuilding, tileCount * 2 - targetTile.GetComponent<TileObject>().tileNum);
                    targetTile.GetComponent<TileObject>().buildingSet = true;
                }
                else {
                    DropSwap(swapTargetBuilding, selectBuilding);
                }
            }
            else {
                DeleteBuilding();
            }
        }
        else {
            //selectBuilding.transform.position = startEditPosition;
            if (picking == true)
                DeleteBuilding();
            else {
                SetSortingOrder(selectBuilding, startSortingOrder);
                selectBuilding.transform.position = startEditPosition;
            }
        }

        clicktime = 0f;
        picking = false;
        SetColor(selectBuilding, Color.white);
        selectBuilding.GetComponent<PolygonCollider2D>().enabled = true;
        swapTargetBuilding = null;
        selectBuilding = null;
        //cam.GetComponent<BitBenderGames.MobileTouchCamera>().enabled = true;        
    }

    public void DeleteBuilding() {
        if (saveSelectBuilding == null)
            return;

        if (saveSelectBuilding.GetComponent<BuildingObject>().data.id == -1)
            return;

        GameObject slot = FindCard(saveSelectBuilding.GetComponent<BuildingObject>().data.id);
        int maxbuildCount = saveSelectBuilding.GetComponent<BuildingObject>().data.card.placementLimit;
        int count = maxbuildCount - OnTileBuildingCount(saveSelectBuilding);
        count++;

        if(count > 0) {
            slot.GetComponent<Image>().color = Color.white;
            slot.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            slot.transform.GetChild(1).GetComponent<Text>().color = Color.white;
            slot.transform.GetChild(2).GetComponent<Text>().color = Color.white;
        }

        //slot.transform.GetChild(2).GetComponent<Text>().text = count.ToString() + " / " + selectbuildingStatus.GetComponent<BuildingObject>().data.card.placementLimit;
        slot.transform.GetChild(2).GetComponent<Text>().text = count.ToString() + " / " + maxbuildCount.ToString();

        GameObject ActiveSkillUISlot = FindActiveSlot(saveSelectBuilding.GetComponent<BuildingObject>().data.id);
        if(ActiveSkillUISlot != null) {
            ClearActiveSlot(ActiveSkillUISlot);
        }



        Cost cost = saveSelectBuilding.GetComponent<BuildingObject>().data.card.product;
        MinusSliderValue(cost);

        tileSetList[saveSelectBuilding.transform.parent.GetComponent<TileObject>().tileNum] = 0;
        saveSelectBuilding.transform.parent.GetComponent<TileObject>().buildingSet = false;
        gameObject.transform.GetChild(3).gameObject.SetActive(false);
        Destroy(saveSelectBuilding);
    }
    

    public void DeleteBuilding(GameObject building) {
        if (building == null)
            return;

        if (building.GetComponent<BuildingObject>().data.id == -1)
            return;

        GameObject card = FindCard(building.GetComponent<BuildingObject>().data.id);
        int maxbuildCount = building.GetComponent<BuildingObject>().data.card.placementLimit;
        int count = maxbuildCount - OnTileBuildingCount(building);
        count++;

        if (count > 0) {
            card.GetComponent<Image>().color = Color.white;
            card.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            card.transform.GetChild(1).GetComponent<Text>().color = Color.white;
            card.transform.GetChild(2).GetComponent<Text>().color = Color.white;
        }

        //card.transform.GetChild(2).GetComponent<Text>().text = count.ToString() + " / " + selectbuildingStatus.GetComponent<BuildingObject>().data.card.placementLimit;
        card.transform.GetChild(2).GetComponent<Text>().text = count.ToString() + " / " + maxbuildCount.ToString();

        GameObject ActiveSkillUISlot = FindActiveSlot(building.GetComponent<BuildingObject>().data.id);
        if (ActiveSkillUISlot != null) {
            ClearActiveSlot(ActiveSkillUISlot);
        }

        Cost cost = building.GetComponent<BuildingObject>().data.card.product;
        MinusSliderValue(cost);

        tileSetList[building.transform.parent.GetComponent<TileObject>().tileNum] = 0;
        building.transform.parent.GetComponent<TileObject>().buildingSet = false;
        gameObject.transform.GetChild(3).gameObject.SetActive(false);
        Destroy(building);
    }

    public void ShowBuildingStatus() {
        //오른쪽 위에 빌딩 정보를 띄우는 함수
        if (saveSelectBuilding == null)
            return;

        gameObject.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = saveSelectBuilding.GetComponent<BuildingObject>().data.card.name; // 이름부분 (canvas => buildingStatus => BuildingName)
        gameObject.transform.GetChild(3).GetChild(1).GetChild(1).GetComponent<Text>().text = saveSelectBuilding.GetComponent<BuildingObject>().data.card.hitPoint.ToString(); // 이름부분 (canvas => buildingStatus => 체력부분)
        gameObject.transform.GetChild(3).gameObject.SetActive(true);

    }

    public int OnTileBuildingCount(GameObject _building) {
        //타일위에 받은 빌딩이 얼마나 있냐?
        int count = 0;

        if (_building == null)
            return count;


        for (int i = 0; i < tileCount; i++) {
            if (tileGroup.transform.GetChild(i).childCount != 0) {
                GameObject compareObject = tileGroup.transform.GetChild(i).GetChild(0).gameObject;

                if (_building.GetComponent<BuildingObject>().data.id == compareObject.GetComponent<BuildingObject>().data.id)
                    count++;
            }
        }
        return count;
    }

    public void CheckCardCount() {
        // start()에서 활용중인데, 배치가 되있는데 카운트가 있으면 안되니, 카운트를 줄이는 함수
        if (cardsContent == null)
            return;

        for(int i = 0; i < cardsContent.transform.childCount; i++) // 페이지 검사
        {
            for(int j = 0; j< cardsContent.transform.GetChild(i).childCount; j++) //  i 페이지 안에 있는 slot 검사
            {
                GameObject slot = cardsContent.transform.GetChild(i).GetChild(j).gameObject; // i페이지 안에 있는 j번째 카드
                //slot.transform.GetChild(2).GetComponent<Text>().text = BuildingCount(slot.GetComponent<DragHandler>().setObject).ToString() + " / " + slot.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().data.card.placementLimit.ToString();
                int maxBuildCount = slot.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().data.card.placementLimit;
                int count = maxBuildCount - OnTileBuildingCount(slot.GetComponent<DragHandler>().setObject);
                if (count == 0) {
                    slot.GetComponent<Image>().color = Color.grey;
                    slot.transform.GetChild(0).GetComponent<Image>().color = Color.grey;
                    slot.transform.GetChild(1).GetComponent<Text>().color = Color.grey;
                    slot.transform.GetChild(2).GetComponent<Text>().color = Color.grey;
                }

                slot.transform.GetChild(2).GetComponent<Text>().text = count.ToString() + " / " + maxBuildCount.ToString();
            }
        }
    }

    public void ResetCardCount() {
        //리셋시에 건물이 사라지니, 건물의 카운트를 재조정.
        if (cardsContent == null)
            return;

        for (int i = 0; i < cardsContent.transform.childCount; i++) // 페이지 검사
        {
            for (int j = 0; j < cardsContent.transform.GetChild(i).childCount; j++) //  i 페이지 안에 있는 slot 검사
            {
                GameObject slot = cardsContent.transform.GetChild(i).GetChild(j).gameObject; // i페이지 안에 있는 j번째 카드
                int maxBuildCount = slot.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().data.card.placementLimit;
                //slot.transform.GetChild(2).GetComponent<Text>().text = 0 + " / " + slot.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().data.card.placementLimit.ToString();
                slot.GetComponent<Image>().color = Color.white;
                slot.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                slot.transform.GetChild(1).GetComponent<Text>().color = Color.white;
                slot.transform.GetChild(2).GetComponent<Text>().text = maxBuildCount.ToString() + " / " + maxBuildCount.ToString();
                slot.transform.GetChild(2).GetComponent<Text>().color = Color.white;
            }
        }
    }

    public GameObject FindCard(int id) {
        //id값을 받아서 activeSlotUI 안에 있는 카드를 리턴해주는 함수
        GameObject card;

        if(cardsContent != null) {
            for(int i = 0; i< cardsContent.transform.childCount; i++) {
                for(int j = 0; j < cardsContent.transform.GetChild(i).childCount; j++) {
                    if (cardsContent.transform.GetChild(i).GetChild(j).GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().data.id == id) {
                        card = cardsContent.transform.GetChild(i).GetChild(j).gameObject;
                        return card;
                    }
                }
            }
        }
        return null;
    }


    public void ChangeSliderValue(Cost cost) {
        sliders[0].value += cost.environment;
        sliders[3].value += cost.gold;
        sliders[1].value += cost.food;
    }

    public void MinusSliderValue(Cost cost) {

        if (cost.environment > 0) {
            if (sliders[0].value > 0)
                sliders[0].value -= cost.environment;
        }
        else {
            if (sliders[0].value > 0)
                sliders[0].value += cost.environment;
        }

        if (cost.gold > 0) {
            if (sliders[3].value > 0)
                sliders[3].value -= cost.gold;
        }
        else {
            if (sliders[3].value > 0)
                sliders[3].value += cost.gold;
        }

        if (cost.food > 0) {
            if (sliders[1].value > 0)
                sliders[1].value -= cost.food;
        }
        else {
            if (sliders[1].value > 0)
                sliders[1].value += cost.food;
        }
    }

    public void ResetAllSliderValues() {
        for(int i=0; i<sliders.Length; i++) {
            sliders[i].value = 0;
        }
    }

    public void OnSliderValueChanged(GameObject slider) {
        slider.transform.Find("Text").GetComponent<Text>().text = slider.GetComponent<Slider>().value.ToString();
    }

    public bool SetAllTileBuildingCheck() {
        // 활성화 된 타일에 카드가 다 올라왔는지 체크하는 함수
        int count = 0;
        bool setComplete = false;

        for(int i = 0; i< tileCount; i++) {
            if (tileGroup.transform.GetChild(i).GetComponent<TileObject>().buildingSet)
                count++;
        }

        if (count > 8)
            setComplete = true;
        else
            setComplete = false;


        return setComplete;
    }

    public void DeckActiveCheck() {
        //건물의 액티브 스킬(현재는 start()에서 활용)
        int slotNum = 0;
        for(int i = 0; i < tileCount; i++) {
            GameObject tile = tileGroup.transform.GetChild(i).gameObject;
            if (tile.GetComponent<TileObject>().buildingSet == true && tile.transform.childCount == 1) {
                GameObject building = tile.transform.GetChild(0).gameObject;
                BuildingObject buildingObject = building.GetComponent<BuildingObject>();
                Building buildingData = buildingObject.data;

                if (buildingObject.data.id == -1)
                    continue;


                if (buildingData.card.activeSkills.Length != 0) {
                    activeSlotUI.transform.GetChild(slotNum).gameObject.SetActive(true);
                    activeSlotUI.transform.GetChild(slotNum).GetComponent<Image>().sprite = buildingObject.mainSprite;
                    activeSlotUI.transform.GetChild(slotNum).GetComponent<ActiveSlot>().id = buildingData.id;
                    activeSlotUI.transform.GetChild(slotNum).GetComponent<ActiveSlot>()._object = building;
                    activeSlotUI.transform.GetChild(slotNum).GetChild(0).GetComponent<Text>().text = buildingData.card.activeSkills[0].name;
                    slotNum++;
                }
                else if(buildingData.card.unit.id != null && buildingData.card.unit.id != "") {
                    activeSlotUI.transform.GetChild(slotNum).gameObject.SetActive(true);
                    activeSlotUI.transform.GetChild(slotNum).GetComponent<Image>().sprite = buildingObject.mainSprite;
                    activeSlotUI.transform.GetChild(slotNum).GetComponent<ActiveSlot>().id = buildingData.id;
                    activeSlotUI.transform.GetChild(slotNum).GetComponent<ActiveSlot>()._object = building;
                    activeSlotUI.transform.GetChild(slotNum).GetChild(0).GetComponent<Text>().text = buildingData.card.unit.name;
                    slotNum++;

                }
            }
        }
    }

    public GameObject FindActiveSlot(int id) {
        // "ID" 값을 받아서, 액티브 슬롯에 있는 정보의 ID가 들어온 ID값과 일치하면 리턴
        GameObject slot;
        for(int i = 0; i < activeSlotUI.transform.childCount; i++) {
            if(activeSlotUI.transform.GetChild(i).GetComponent<ActiveSlot>().id == id) {
                slot = activeSlotUI.transform.GetChild(i).gameObject;
                return slot;
            }
        }
        return null;
    }

    public GameObject FindActiveNullSlot() {
        //무조건적으로 비어있는 슬롯 찾기.
        GameObject slot;
        for (int i = 0; i < activeSlotUI.transform.childCount; i++) {
            if(activeSlotUI.transform.GetChild(i).GetComponent<ActiveSlot>().id == 0 && activeSlotUI.transform.GetChild(i).gameObject.activeSelf == false) {
                return activeSlotUI.transform.GetChild(i).gameObject;
            }
        }
        return null;
    }

    public void ClearActiveSlot(GameObject _slot) {
        //FindActiveSlot으로 받은 카드를 대입시켜서, activeslot을 지우는 함수
        GameObject slot;
        slot = _slot;
        slot.GetComponent<Image>().sprite = null;
        slot.GetComponent<ActiveSlot>().id = 0;
        slot.GetComponent<ActiveSlot>()._object = null;
        slot.transform.GetChild(0).GetComponent<Text>().text = " ";
        slot.SetActive(false);
    }

    public void ResetActiveSlot() {
        //리셋 버튼 눌렀을때 액티브 슬롯을 전부 지우는 함수
        GameObject slot;
        for (int i = 0; i < activeSlotUI.transform.childCount; i++) {
            slot = activeSlotUI.transform.GetChild(i).gameObject;
            /*
            if (slot.GetComponent<ActiveSlot>().id == -1) {
                if (i == 0)  continue;
                //HQ처리
                activeSlotUI.transform.GetChild(0).GetComponent<ActiveSlot>().id = slot.GetComponent<ActiveSlot>().id;
                activeSlotUI.transform.GetChild(0).GetComponent<ActiveSlot>()._object = slot.GetComponent<ActiveSlot>()._object;
                activeSlotUI.transform.GetChild(0).GetComponent<Image>().sprite = slot.GetComponent<Image>().sprite;
                activeSlotUI.transform.GetChild(0).gameObject.SetActive(true);
            }
            */
            slot.GetComponent<Image>().sprite = null;
            slot.GetComponent<ActiveSlot>().id = 0;
            slot.GetComponent<ActiveSlot>()._object = null;
            slot.transform.GetChild(0).GetComponent<Text>().text = " ";
            slot.SetActive(false);
        }
    }

    public void AddActiveSlot(GameObject _building) {
        
        //드랍 핸들러에서, 건물 배치 조건이 될 시에, 건물을 받아서 액티브 슬롯에 추가.
        GameObject slot = FindActiveNullSlot();
        ActiveSlot activeSlot = slot.GetComponent<ActiveSlot>();
        BuildingObject buildingObject = _building.GetComponent<BuildingObject>();
        Building buildingData = buildingObject.data;
        
        
        if (activeSlot != null) {
            if (buildingData.card.activeSkills.Length != 0) {
                slot.GetComponent<Image>().sprite = buildingObject.mainSprite;
                activeSlot.id = buildingData.id;
                activeSlot._object = _building;
                slot.transform.GetChild(0).GetComponent<Text>().text = buildingData.card.activeSkills[0].name;
                slot.SetActive(true);
            }
            
            else if (buildingData.card.unit.id != null && buildingData.card.unit.id != "") {
                slot.GetComponent<Image>().sprite = buildingObject.mainSprite;
                activeSlot.id = buildingData.id;
                activeSlot._object = _building;
                slot.transform.GetChild(0).GetComponent<Text>().text = buildingData.card.unit.name;
                slot.SetActive(true);
            }
        }


    }
    private void SetColor(GameObject setBuilding, Color color) {
        SpriteRenderer spriteRenderer = setBuilding.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            spriteRenderer.color = color;
        }
        else {
            setBuilding.GetComponent<SkeletonAnimation>().skeleton.SetColor(color);
        }
    }

    private void SetSortingOrder(GameObject setBuilding, int order) {
        SpriteRenderer spriteRenderer = setBuilding.GetComponent<SpriteRenderer>();
        if(spriteRenderer != null) {
            spriteRenderer.sortingOrder = order;
        }
        else {
            setBuilding.GetComponent<MeshRenderer>().sortingOrder = order;
        }
    }

    private int GetSortingOrder(GameObject setBuilding) {
        SpriteRenderer spriteRenderer = setBuilding.GetComponent<SpriteRenderer>();
        if(spriteRenderer != null) return spriteRenderer.sortingOrder;
        else return setBuilding.GetComponent<MeshRenderer>().sortingOrder;
    }

    public void DragSwap(GameObject onTilebuildingObject) {
        if(onTilebuildingObject == null) {
            if (swapTargetBuilding != null)
                swapTargetBuilding.SetActive(true);

            swapTargetBuilding = null;
            return;
        }

        if (swapTargetBuilding != null && swapTargetBuilding != onTilebuildingObject) 
            swapTargetBuilding.SetActive(true);            
        
        swapTargetBuilding = onTilebuildingObject;
        swapTargetBuilding.SetActive(false);
    }

    public void DropSwap(GameObject swapBuilding, GameObject pickBuilding) {
        if(swapBuilding == null && pickBuilding != null) {
            SetSortingOrder(pickBuilding, startSortingOrder);
            return;
        }

        if (swapBuilding == null || pickBuilding == null)
            return;

        if (swapBuilding.GetComponent<BuildingObject>().data.id == -1) {
            SetSortingOrder(pickBuilding, startSortingOrder);
            pickBuilding.transform.position = startEditPosition;
            swapBuilding.SetActive(true);
            return;
        }

        if(swapBuilding.GetComponent<BuildingObject>().data.id == pickBuilding.GetComponent<BuildingObject>().data.id) {
            SetSortingOrder(pickBuilding, startSortingOrder);
            pickBuilding.transform.position = startEditPosition;
            swapBuilding.SetActive(true);
            return;
        }

        GameObject swapTile;
        GameObject recentTile;

        int swapOrder;
        int recentOrder;

        swapBuilding.SetActive(true);

        swapTile = swapBuilding.transform.parent.gameObject;
        recentTile = pickBuilding.transform.parent.gameObject;

        swapOrder = GetSortingOrder(swapBuilding);
        recentOrder = startSortingOrder;

        pickBuilding.transform.SetParent(swapTile.transform);
        swapBuilding.transform.SetParent(recentTile.transform);

        pickBuilding.transform.position = swapTile.transform.position;
        swapBuilding.transform.position = recentTile.transform.position;

        int temp = tileSetList[recentTile.GetComponent<TileObject>().tileNum];
        tileSetList[recentTile.GetComponent<TileObject>().tileNum] = tileSetList[swapTile.GetComponent<TileObject>().tileNum];
        tileSetList[swapTile.GetComponent<TileObject>().tileNum] = temp;


        SetSortingOrder(pickBuilding, swapOrder);
        SetSortingOrder(swapBuilding, recentOrder);
    }

    private void ShowDetail(BuildingObject buildingObject) {
        if (buildingObject.data.card.unit == null || string.IsNullOrEmpty(buildingObject.data.card.unit.name)) {
            prodDetailModal.SetActive(true);
            Transform innerModal = prodDetailModal.transform.GetChild(0);

            Text hp = innerModal.Find("DataArea/UpperBody/HP/Value").GetComponent<Text>();
            Text header = innerModal.Find("Header/Text").GetComponent<Text>();
            Text limitCount = innerModal.Find("Upper/LimitCount/Value").GetComponent<Text>();
            Text tier = innerModal.Find("Upper/DataArea/Header/TierName").GetComponent<Text>();

            Text food = innerModal.Find("DataArea/UpperBody/Food/Value").GetComponent<Text>();
            Text env = innerModal.Find("DataArea/UpperBody/Env/Value").GetComponent<Text>();
            Text gold = innerModal.Find("DataArea/UpperBody/Gold/Value").GetComponent<Text>();

            Card card = buildingObject.data.card;
            hp.text = card.hitPoint.ToString();
            header.text = card.name;
            limitCount.text = "한도 " + card.placementLimit.ToString();

            tier.text = card.rarity + " 등급";
            food.text = card.product.food.ToString();
            gold.text = card.product.gold.ToString();
            env.text = card.product.environment.ToString();

            Image image = innerModal.Find("Upper/ImageArea/Image").GetComponent<Image>();
            image.sprite = ConstructManager.Instance.GetComponent<BuildingImages>().GetImage(buildingObject.data.card.race, buildingObject.data.card.type, buildingObject.data.card.id);
            prodDetailModal.transform.GetChild(0).GetChild(4).GetComponent<Button>().OnClickAsObservable().Subscribe(_ => DeleteBuilding(saveSelectBuilding));
            prodDetailModal.transform.GetChild(0).GetChild(4).GetComponent<Button>().OnClickAsObservable().Subscribe(_ => prodDetailModal.SetActive(false));
        }
        else {
            unitGenDetailModal.SetActive(true);
            Transform innerModal = unitGenDetailModal.transform.GetChild(0);

            Text tier = innerModal.Find("Upper/DataArea/Header/TierName").GetComponent<Text>();
            Text header = innerModal.Find("Header/Text").GetComponent<Text>();

            Text unitName = innerModal.Find("DataArea/UpperBody/Text").GetComponent<Text>();
            Text needResources = innerModal.Find("DataArea/UpperBody/NeedResource").GetComponent<Text>();
            Text unitSpec = innerModal.Find("DataArea/BottomBody/UnitSpec").GetComponent<Text>();

            Card card = buildingObject.data.card;
            DataModules.Unit unit = card.unit;

            tier.text = unit.tierNeed + " 등급";
            header.text = card.name;

            unitName.text = "유닛생산 " + unit.name;
            Debug.Log(tier.text);
            needResources.text = "식량 : " + unit.cost.food + "\n"
                + "골드 : " + unit.cost.gold + "\n"
                + "환경 : " + unit.cost.environment + "\n";

            unitSpec.text = "체력 : " + unit.hitPoint + "\n"
                + "공격력 : " + unit.power + "\n"
                + "공격 속도 : " + unit.attackSpeed + "\n"
                + "공격 범위 : " + unit.attackRange + "\n"
                + "이동 속도 : " + unit.moveSpeed + "\n"
                + "요구 레벨 : " + unit.tierNeed;

            Image image = innerModal.Find("Upper/ImageArea/Image").GetComponent<Image>();
            image.sprite = ConstructManager.Instance.GetComponent<BuildingImages>().GetImage(buildingObject.data.card.race, buildingObject.data.card.type, buildingObject.data.card.id);
            unitGenDetailModal.transform.GetChild(0).GetChild(4).GetComponent<Button>().OnClickAsObservable().Subscribe(_ => DeleteBuilding(saveSelectBuilding));
            unitGenDetailModal.transform.GetChild(0).GetChild(4).GetComponent<Button>().OnClickAsObservable().Subscribe(_ => unitGenDetailModal.SetActive(false));
        }
        Debug.Log(prodDetailModal.transform.GetChild(0).GetChild(4).name);
        

        selectBuilding = null;
    }

    public void TimeCounting() {
        

    }
}
