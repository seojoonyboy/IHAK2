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
    [SerializeField] private EditScenePanel editScenePanel;
    [SerializeField] public GameObject DeckStatusUI;
    [SerializeField] public GameObject selectUI;

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
    public bool nameEditing = false;

    [Header(" - UISlider")]
    public Slider[] sliders;
    public GameObject radialfillGauge;

    [Header(" - UserData")]
    private int speciesId = 0;
    private int deckCount;
    public int buildingCount = 0;

    [Header(" - Time")]
    public float clicktime = 0f;
    public float requireClickTime = 0.3f;

    [Header(" - ResourceState")]
    public int food;
    public decimal environment;
    public decimal gold;

    [Header(" - CardSort")]
    public List<GameObject> totalCard;
    public GameObject UnpopCardPage;
    public GameObject cardContent;
    private const int NUM_PER_PAGE = 8;
    public int maxbuildCount = 1;

    [Header(" - DeckNameEdit")]    
    public Button nameEditBtn;
    public string changedDeckName;

    [Header(" - BuildingMove")]
    public bool moveBuild = false;
    public GameObject moveTarget;


    public int SpeciesId {
        get {
            return speciesId;
        }
    }
    
    

    private void Start() {
        playerInfosManager = AccountManager.Instance;
        constructManager = ConstructManager.Instance;
        cardsContent = transform.GetChild(0).Find("Content").gameObject; // Canvas => UnitScrollPanel => Content;
        nameEditBtn = DeckStatusUI.transform.Find("DeckNameEditBtn").GetComponent<Button>();
        //activeSlotUI = transform.GetChild(3).GetChild(0).gameObject; // Canvas => ActiveEffectPanel => Content;
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
        resetButton.OnClickAsObservable().Subscribe(_ => {
            Modal.instantiate("장착 중인 모든 카드를 해체합니다.", Modal.Type.YESNO, () => resetTile());
        });
        deleteButton.OnClickAsObservable().Subscribe(_ => DeleteBuilding());

        nameEditBtn.OnClickAsObservable().Where(_ => nameEditing == false).Subscribe(_ => InputDeckName());
     

        downStream.Subscribe(_ => PickEditBuilding());
        dragStream.Where(_ => (clicktime < requireClickTime) && (picking== true || selectBuilding != null)).Subscribe(_ => clicktime += Time.deltaTime);
        dragStream.Where(_=>clicktime >= requireClickTime).Subscribe(_ => MoveEditBuilding());
        upStream.Where(_ => clicktime < requireClickTime && selectBuilding != null).Subscribe(_=> ShowDetail(selectBuilding.GetComponent<BuildingObject>()));
        upStream.Where(_ => clicktime < requireClickTime && selectBuilding != null).Subscribe(_ => selectBuilding.GetComponent<PolygonCollider2D>().enabled = true);
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
                    product = buildingObject.card.data.product;
                    if (product != null) {
                        ChangeSliderValue(product);
                    }
                }
            }
            FirstSetDeckInfo();
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
    public void FirstSetDeckInfo() {
        Text deckName = DeckStatusUI.transform.Find("DeckName").GetComponent<Text>();
        Text deckBuildingCount = DeckStatusUI.transform.Find("DeckBuildingCount").GetComponent<Text>();
        int deckNumber = playerInfosManager.selectNumber;
        Text showFieldName = DeckStatusUI.transform.Find("EditField").Find("DeckNameInputField").GetChild(0).GetComponent<Text>();

        changedDeckName = deckName.text = playerInfosManager.decks[deckNumber].name;
        showFieldName.text = deckName.text;
        deckBuildingCount.text = buildingCount.ToString() + " / " + 8.ToString();
    }

    public void SetDeckInfo() {
        Text deckBuildingCount = DeckStatusUI.transform.Find("DeckBuildingCount").GetComponent<Text>();
        deckBuildingCount.text = buildingCount.ToString() + " / " + 8.ToString();
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
        if (SetAllTileBuildingCheck()) {
            if (changedDeckName == "")
                Modal.instantiate("이름을 입력해주세요.", Modal.Type.CHECK);
            else
                OnclickInputConfirm(changedDeckName);
        }
        else
            Modal.instantiate("건물을 모두 배치해주세요.", Modal.Type.CHECK);
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
            if (tileGroup.transform.GetChild(i).childCount != 0) {
                tileSetList.Add(tileGroup.transform.GetChild(i).GetChild(0).GetComponent<BuildingObject>().card.id);
                ChangeSliderValue(tileGroup.transform.GetChild(i).GetChild(0).GetComponent<BuildingObject>().card.data.product);

                if (tileGroup.transform.GetChild(i).GetChild(0).GetComponent<BuildingObject>().setTileLocation != 12) {
                    buildingCount++;
                }
                SetDeckInfo();
            }
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
        buildingCount = 0;
        SetDeckInfo();
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
                if (selectBuilding.GetComponent<BuildingObject>().setTileLocation == 12)
                    selectBuilding = null;

            }
            else if (hit.collider.tag == "Tile") {
                if (hit.transform.gameObject.transform.childCount != 0) {
                    selectBuilding = hit.transform.GetChild(0).gameObject;
                    saveSelectBuilding = selectBuilding;
                    startSortingOrder = GetSortingOrder(selectBuilding);
                    if (selectBuilding.GetComponent<BuildingObject>().setTileLocation == 12)
                        selectBuilding = null;
                }
                else {
                    //gameObject.transform.GetChild(2).gameObject.SetActive(false);
                    return;
                }
            }
            else if (hit.collider.tag == "BackGroundTile") {
               // gameObject.transform.GetChild(2).gameObject.SetActive(false);
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
                    buildingCount++;
                    SetDeckInfo();
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

        if (saveSelectBuilding.GetComponent<BuildingObject>().setTileLocation == 12)
            return;

        GameObject slot = FindCard(saveSelectBuilding.GetComponent<BuildingObject>().card.id);
        //int maxbuildCount = saveSelectBuilding.GetComponent<BuildingObject>().card.data.placementLimit;
        
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

        GameObject ActiveSkillUISlot = FindActiveSlot(saveSelectBuilding.GetComponent<BuildingObject>().card.id);
        if(ActiveSkillUISlot != null) {
            ClearActiveSlot(ActiveSkillUISlot);
        }



        Cost cost = saveSelectBuilding.GetComponent<BuildingObject>().card.data.product;
        MinusSliderValue(cost);

        tileSetList[saveSelectBuilding.transform.parent.GetComponent<TileObject>().tileNum] = 0;
        saveSelectBuilding.transform.parent.GetComponent<TileObject>().buildingSet = false;
        //gameObject.transform.GetChild(2).gameObject.SetActive(false);
        buildingCount--;
        SetDeckInfo();
        Destroy(saveSelectBuilding);
    }
    

    public void DeleteBuilding(GameObject building) {
        if (building == null)
            return;

        if (building.GetComponent<BuildingObject>().setTileLocation == 12)
            return;

        GameObject card = FindCard(building.GetComponent<BuildingObject>().card.id);
        //int maxbuildCount = building.GetComponent<BuildingObject>().card.data.placementLimit;
        //int maxbuildCount = 2;
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

        GameObject ActiveSkillUISlot = FindActiveSlot(building.GetComponent<BuildingObject>().card.id);
        if (ActiveSkillUISlot != null) {
            ClearActiveSlot(ActiveSkillUISlot);
        }

        Cost cost = building.GetComponent<BuildingObject>().card.data.product;
        MinusSliderValue(cost);

        tileSetList[building.transform.parent.GetComponent<TileObject>().tileNum] = 0;
        building.transform.parent.GetComponent<TileObject>().buildingSet = false;
        //gameObject.transform.GetChild(2).gameObject.SetActive(false);
        buildingCount--;
        SetDeckInfo();
        Destroy(building);
    }

    public void ShowBuildingStatus(GameObject cardSetObject) {
        //오른쪽 위에 빌딩 정보를 띄우는 함수
        if (cardSetObject == null)
            return;

        GameObject informationObject = transform.GetChild(2).gameObject;

        informationObject.transform.GetChild(0).GetComponent<Text>().text = cardSetObject.GetComponent<BuildingObject>().card.data.name; // 이름부분 (canvas => buildingStatus => BuildingName)
        informationObject.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = cardSetObject.GetComponent<BuildingObject>().card.data.hitPoint.ToString(); // 이름부분 (canvas => buildingStatus => 체력부분)
        informationObject.gameObject.SetActive(true);
    }

    public void CloseBuildingStatus() {
        //gameObject.transform.GetChild(2).gameObject.SetActive(false);
    }

    public int OnTileBuildingCount(GameObject _building) {
        //타일위에 받은 빌딩이 얼마나 있냐?
        int count = 0;

        if (_building == null)
            return count;


        for (int i = 0; i < tileCount; i++) {
            if (tileGroup.transform.GetChild(i).childCount != 0) {
                GameObject compareObject = tileGroup.transform.GetChild(i).GetChild(0).gameObject;

                if (_building.GetComponent<BuildingObject>().card.id == compareObject.GetComponent<BuildingObject>().card.id)
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
                //int maxBuildCount = slot.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().card.data.placementLimit;
                //int maxBuildCount = 1;
                int count = maxbuildCount - OnTileBuildingCount(slot.GetComponent<DragHandler>().setObject);
                if (count == 0) {
                    slot.GetComponent<Image>().color = Color.grey;
                    slot.transform.GetChild(0).GetComponent<Image>().color = Color.grey;
                    slot.transform.GetChild(1).GetComponent<Text>().color = Color.grey;
                    slot.transform.GetChild(2).GetComponent<Text>().color = Color.grey;
                }

                slot.transform.GetChild(2).GetComponent<Text>().text = count.ToString() + " / " + maxbuildCount.ToString();
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
                //int maxBuildCount = slot.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().card.data.placementLimit;
                //int maxBuildCount = 2;
                //slot.transform.GetChild(2).GetComponent<Text>().text = 0 + " / " + slot.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().data.card.placementLimit.ToString();
                slot.GetComponent<Image>().color = Color.white;
                slot.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                slot.transform.GetChild(1).GetComponent<Text>().color = Color.white;
                slot.transform.GetChild(2).GetComponent<Text>().text = maxbuildCount.ToString() + " / " + maxbuildCount.ToString();
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
                    if (cardsContent.transform.GetChild(i).GetChild(j).GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().card.id == id) {
                        card = cardsContent.transform.GetChild(i).GetChild(j).gameObject;
                        return card;
                    }
                }
            }
        }
        return null;
    }


    public void ChangeSliderValue(Cost cost) {
        //food += cost.food;
        environment += cost.environment;
        gold += cost.gold;

        if (environment > 0)
            sliders[0].value = (float)environment;
        else
            sliders[0].value = 0;

        if (gold > 0)
            sliders[3].value = (float)gold;
        else
            sliders[3].value = 0;

        if (food > 0)
            sliders[1].value = food;
        else
            sliders[1].value = 0;
    }

    public void MinusSliderValue(Cost cost) {

        environment -= cost.environment;
        gold -= cost.gold;
        //food -= cost.food;

        if (environment > 0)
            sliders[0].value = (float)environment;
        else
            sliders[0].value = 0;

        if (gold > 0)
            sliders[3].value = (float)gold;
        else
            sliders[3].value = 0;

        if (food > 0)
            sliders[1].value = food;
        else
            sliders[1].value = 0;
    }

    public void ResetAllSliderValues() {
        for(int i=0; i<sliders.Length; i++) {
            sliders[i].value = 0;
        }

        food = 0;
        environment = 0;
        gold = 0;
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
                Card buildingData = buildingObject.card;
                GameObject activeSlot = activeSlotUI.transform.GetChild(0).GetChild(slotNum).gameObject;

                if (buildingObject.card.id == -1)
                    continue;


                if (buildingData.data.activeSkills.Length != 0) {
                    activeSlot.SetActive(true);
                    activeSlot.GetComponent<Image>().sprite = buildingObject.mainSprite;
                    activeSlot.GetComponent<ActiveSlot>().id = buildingData.id;
                    activeSlot.GetComponent<ActiveSlot>()._object = building;
                    activeSlot.transform.GetChild(0).GetComponent<Text>().text = buildingData.data.activeSkills[0].name;
                    slotNum++;
                }
                else if(buildingData.data.unit.id != null && buildingData.data.unit.id != "") {
                    activeSlot.SetActive(true);
                    activeSlot.GetComponent<Image>().sprite = buildingObject.mainSprite;
                    activeSlot.GetComponent<ActiveSlot>().id = buildingData.id;
                    activeSlot.GetComponent<ActiveSlot>()._object = building;
                    activeSlot.transform.GetChild(0).GetComponent<Text>().text = buildingData.data.unit.name;
                    slotNum++;

                }
            }
        }
    }

    public GameObject FindActiveSlot(int id) {
        // "ID" 값을 받아서, 액티브 슬롯에 있는 정보의 ID가 들어온 ID값과 일치하면 리턴
        GameObject slot;
        for(int i = 0; i < activeSlotUI.transform.GetChild(0).childCount; i++) {
            if(activeSlotUI.transform.GetChild(0).GetChild(i).GetComponent<ActiveSlot>().id == id) {
                slot = activeSlotUI.transform.GetChild(0).GetChild(i).gameObject;
                return slot;
            }
        }
        return null;
    }

    public GameObject FindActiveNullSlot() {
        //무조건적으로 비어있는 슬롯 찾기.
        for (int i = 0; i < activeSlotUI.transform.GetChild(0).childCount; i++) {
            if(activeSlotUI.transform.GetChild(0).GetChild(i).GetComponent<ActiveSlot>().id == 0 && activeSlotUI.transform.GetChild(0).GetChild(i).gameObject.activeSelf == false) {
                return activeSlotUI.transform.GetChild(0).GetChild(i).gameObject;
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
        for (int i = 0; i < activeSlotUI.transform.GetChild(0).childCount; i++) {
            slot = activeSlotUI.transform.GetChild(0).GetChild(i).gameObject;
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
        Card buildingData = buildingObject.card;
        
        
        if (activeSlot != null) {
            if (buildingData.data.activeSkills.Length != 0) {
                slot.GetComponent<Image>().sprite = buildingObject.mainSprite;
                activeSlot.id = buildingData.id;
                activeSlot._object = _building;
                slot.transform.GetChild(0).GetComponent<Text>().text = buildingData.data.activeSkills[0].name;
                slot.SetActive(true);
            }
            
            else if (buildingData.data.unit.id != null && buildingData.data.unit.id != "") {
                slot.GetComponent<Image>().sprite = buildingObject.mainSprite;
                activeSlot.id = buildingData.id;
                activeSlot._object = _building;
                slot.transform.GetChild(0).GetComponent<Text>().text = buildingData.data.unit.name;
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

        if (swapBuilding.GetComponent<BuildingObject>().card.id == -1) {
            SetSortingOrder(pickBuilding, startSortingOrder);
            pickBuilding.transform.position = startEditPosition;
            swapBuilding.SetActive(true);
            return;
        }

        if(swapBuilding.GetComponent<BuildingObject>().card.id == pickBuilding.GetComponent<BuildingObject>().card.id) {
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
        if (!editScenePanel.cool) return;

        if (buildingObject.card.data.unit == null || string.IsNullOrEmpty(buildingObject.card.data.unit.name)) {
            prodDetailModal.SetActive(true);
            Transform innerModal = prodDetailModal.transform.GetChild(0);

            Text hp = innerModal.Find("DataArea/UpperBody/HP/Value").GetComponent<Text>();
            Text header = innerModal.Find("Header/Text").GetComponent<Text>();
            Text limitCount = innerModal.Find("Upper/LimitCount/Value").GetComponent<Text>();
            Text tier = innerModal.Find("Upper/DataArea/Header/TierName").GetComponent<Text>();

            Text food = innerModal.Find("DataArea/UpperBody/Food/Value").GetComponent<Text>();
            Text env = innerModal.Find("DataArea/UpperBody/Env/Value").GetComponent<Text>();
            Text gold = innerModal.Find("DataArea/UpperBody/Gold/Value").GetComponent<Text>();

            CardData card = buildingObject.card.data;
            hp.text = card.hitPoint.ToString();
            header.text = card.name;
            limitCount.text = "한도 " + card.placementLimit.ToString();

            tier.text = card.rarity + " 등급";
            //food.text = card.product.food.ToString();
            gold.text = card.product.gold.ToString();
            env.text = card.product.environment.ToString();

            Image image = innerModal.Find("Upper/ImageArea/Image").GetComponent<Image>();
            image.sprite = ConstructManager.Instance.GetComponent<BuildingImages>().GetIcon(buildingObject.card.data.race, buildingObject.card.data.type, buildingObject.card.data.id);
            prodDetailModal.transform.GetChild(0).GetChild(4).gameObject.SetActive(true);
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

            Card card = buildingObject.card;
            DataModules.Unit unit = card.data.unit;

            //tier.text = unit. + " 등급";
            header.text = card.data.name;

            unitName.text = "유닛생산 " + unit.name;
            Debug.Log(tier.text);
            needResources.text = "골드 : " + unit.cost.gold + "\n"
                + "환경 : " + unit.cost.environment + "\n";

            unitSpec.text = "체력 : " + unit.hitPoint + "\n"
                + "공격력 : " + unit.power + "\n"
                + "공격 속도 : " + unit.attackSpeed + "\n"
                + "공격 범위 : " + unit.attackRange + "\n"
                + "이동 속도 : " + unit.moveSpeed + "\n";
                //+ "요구 레벨 : " + unit.tierNeed;

            Image image = innerModal.Find("Upper/ImageArea/Image").GetComponent<Image>();
            image.sprite = ConstructManager.Instance.GetComponent<BuildingImages>().GetIcon(buildingObject.card.data.race, buildingObject.card.data.type, buildingObject.card.data.id);
            unitGenDetailModal.transform.GetChild(0).GetChild(4).gameObject.SetActive(true);
            unitGenDetailModal.transform.GetChild(0).GetChild(4).GetComponent<Button>().OnClickAsObservable().Subscribe(_ => DeleteBuilding(saveSelectBuilding));
            unitGenDetailModal.transform.GetChild(0).GetChild(4).GetComponent<Button>().OnClickAsObservable().Subscribe(_ => unitGenDetailModal.SetActive(false));
        }
        Debug.Log(prodDetailModal.transform.GetChild(0).GetChild(4).name);
        

        selectBuilding = null;
    }
    /*
    public void ClickTotalTap() {
        for(int i = 0; i < totalCard.Count; i++) {
            GameObject card = totalCard[i];
            GameObject pageObject = card.GetComponent<DragHandler>().parentPageObject;
            card.transform.SetParent(pageObject.transform);
            card.transform.SetSiblingIndex(card.GetComponent<DragHandler>().sibilingData);
            card.SetActive(true);
        }     

    }

    public void ClickProdTap() {
        SetCardSort("prod");
    }

    public void ClickMilitaryTap() {
        SetCardSort("military");
    }

    public void ClickSpecialTap() {
        SetCardSort("special");
    }

    public void SetCardSort(string productType) {
        int count = 0;
        int page = 0;
        for (int i = 0; i < totalCard.Count; i++) {
            GameObject card = totalCard[i];
            BuildingObject buildingObject = card.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>();
            Transform pageTransform = cardContent.transform.GetChild(page);

            if (buildingObject.data.card.type == productType) {
                card.transform.SetParent(pageTransform);
                card.SetActive(true);
                count++;
                if(count == NUM_PER_PAGE) {
                    count = 0;
                    page++;
                }
            }
            else {
                card.transform.SetParent(UnpopCardPage.transform);
                card.SetActive(false);
            }
        }
    }*/

    public void InputDeckName() {
        GameObject editPanel = DeckStatusUI.transform.Find("EditField").gameObject;
        GameObject nameField = DeckStatusUI.transform.Find("EditField").Find("DeckNameInputField").gameObject;
        Text originalDeckName = DeckStatusUI.transform.Find("DeckName").GetComponent<Text>();
        Text editDeckName = nameField.transform.Find("Text").GetComponent<Text>();

        nameEditing = true;
        editPanel.SetActive(true);
        
    }

    public void SetDeckName() {
        if (nameEditing == false) return;
        GameObject editPanel = DeckStatusUI.transform.Find("EditField").gameObject;
        GameObject nameField = DeckStatusUI.transform.Find("EditField").Find("DeckNameInputField").gameObject;        
        Text showDeckName = DeckStatusUI.transform.Find("DeckName").GetComponent<Text>();
        Text fieldShowDeckName = nameField.transform.Find("Placeholder").GetComponent<Text>();
        string deckName = nameField.transform.Find("Text").GetComponent<Text>().text;
        InputField inputFieldText = editPanel.transform.Find("DeckNameInputField").GetComponent<InputField>();

        editPanel.SetActive(false);

        if (deckName.Length > 0) {
            showDeckName.text = deckName;
            fieldShowDeckName.text = deckName;
            changedDeckName = deckName;
        }
        inputFieldText.text = "";
        nameEditing = false;

        AccountManager.Instance.ModifyDeckName(prevData, changedDeckName);
    }

    public void CloseInputField() {
        if (nameEditing == false) return;
        GameObject editPanel = DeckStatusUI.transform.Find("EditField").gameObject;
        InputField inputFieldText = editPanel.transform.Find("DeckNameInputField").GetComponent<InputField>();

        inputFieldText.text = "";
        editPanel.SetActive(false);
        nameEditing = false;
    }

}
