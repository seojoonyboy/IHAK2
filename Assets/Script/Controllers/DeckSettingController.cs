using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using DataModules;
using System;
using UniRx;
using UniRx.Triggers;


public class DeckSettingController : MonoBehaviour {
    GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.DeckSettingScene;
    GameSceneManager gsm;

    AccountManager playerInfosManager;
    ConstructManager constructManager;

    Camera cam;
    [SerializeField] public GameObject tileGroup;
    [SerializeField] private GameObject togglePref;
    [SerializeField] private Button chooseSpeciesBtn;
    [SerializeField] private Button speciesConfirmBtn;
    [SerializeField] public Button deleteButton;
    [SerializeField] private GameObject modal;
    [SerializeField] Sprite[] speciesPortraits;
    [SerializeField] public GameObject uicontent;
    [SerializeField] DeckListController deckListController;
    [SerializeField] public List<int> tileSetList;
    [SerializeField] public Button resetButton;
    [SerializeField] public bool reset = false;
    [SerializeField] public bool modify;
    [SerializeField] public GameObject selectBuilding;
    [SerializeField] public GameObject selectbuildingStatus;
    [SerializeField] public GameObject targetTile;
    [SerializeField] public Vector3 startEditPosition;
    [SerializeField] public bool picking = false;

    public Text 
        modalHeader,
        content;

    [Header("UISlider")]
    public Slider[] sliders;

    private int speciesId = 0;
    private int deckCount;
    public int SpeciesId {
        get {
            return speciesId;
        }
    }

    public static Deck prevData = null;

    private void Start() {
        playerInfosManager = AccountManager.Instance;
        constructManager = ConstructManager.Instance;
        uicontent = transform.GetChild(0).GetChild(0).gameObject; // Canvas => UnitScrollPanel => Content;        
        deckCount = playerInfosManager.decks.Count;
        gsm = FindObjectOfType<GameSceneManager>();
        cam = Camera.main;
        var downStream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));
        var dragStream = Observable.EveryUpdate().Where(_ => Input.GetMouseButton(0));
        var upStream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonUp(0));
        playerInfosManager.transform.GetChild(0).GetChild(playerInfosManager.selectNumber).gameObject.SetActive(true);
        tileGroup = playerInfosManager.transform.gameObject.transform.GetChild(0).GetChild(playerInfosManager.selectNumber).gameObject;
        deckListController = FindObjectOfType<DeckListController>();
        TilebuildingList();
        CheckUICardCount();
        
        resetButton.OnClickAsObservable().Subscribe(_ => resetTile());
        deleteButton.OnClickAsObservable().Subscribe(_ => DeleteBuilding());

        /* 테스트용
        downStream.Subscribe(_ => Debug.Log("원클릭"));
        dragStream.Delay(TimeSpan.FromMilliseconds(500)).Subscribe(_ => Debug.Log("FromMillSecond500클릭"));
        */

        downStream.Subscribe(_ => PickEditBuilding());
        dragStream.Delay(TimeSpan.FromMilliseconds(500)).Subscribe(_ => MoveEditBuilding());
        upStream.Subscribe(_ => DropEditBuilding());
        
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

    public void settingButton() {
        if (prevData == null) {
            GameObject modal = Modal.instantiate("덱 이름 설정", "덱 이름을 입력해주세요", null, Modal.Type.INSERT, Callback);
            modal.transform.Find("ModalWindow/Modal/Top/Insert/InputField").GetComponent<InputField>().characterLimit = 8;
        }
        else {
            string name = prevData.name;
            string str = name;
            if (name.Length > 8) {
                str = name.Substring(0, 8);
            }
            GameObject modal = Modal.instantiate("덱 이름 설정", null, str, Modal.Type.INSERT, Callback);
            modal.transform.Find("ModalWindow/Modal/Top/Insert/InputField").GetComponent<InputField>().characterLimit = 8;
        }
    }

    private void Callback(string inputText) {
        Deck deck = new Deck();
        deck.race = ((Species.Type)speciesId).ToString();
        deck.name = inputText;
        deck.coordsSerial = new int[tileSetList.Count + 1];
        for (int i = 0; i < tileSetList.Count; i++) {
            deck.coordsSerial[i] = tileSetList[i];
            if (tileGroup.transform.GetChild(i).childCount != 0)
                tileGroup.transform.GetChild(i).GetChild(0).GetComponent<BuildingObject>().setTileLocation = tileGroup.transform.GetChild(i).GetComponent<TileObject>().tileNum;
        }
        deck.coordsSerial = new int[tileSetList.Count];
        if (prevData == null) {
            playerInfosManager.AddDeck(deck);
        }
        else {
            deck.id = prevData.id;
            playerInfosManager.decks[playerInfosManager.selectNumber] = deck;
            playerInfosManager.ModifyDeck(deck);
        }
        prevData = null;
        tileGroup.SetActive(false);
        gsm.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
    }

    public void returnButton() {
        prevData = null;

        if (reset == false) {
            for (int i = 0; i < tileGroup.transform.childCount; i++) {
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
            else
                playerInfosManager.SetTileObjects(playerInfosManager.selectNumber);
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
        playerInfosManager.checkDeck(playerInfosManager.selectNumber);
        gsm.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
    }

    public void TilebuildingList() {
        for (int i = 0; i < tileGroup.transform.childCount; i++) {
            if (tileGroup.transform.GetChild(i).childCount != 0)
                tileSetList.Add(tileGroup.transform.GetChild(i).GetChild(0).GetComponent<BuildingObject>().data.id);
            else
                tileSetList.Add(0);
        }
    }

    public void resetTile() {
        for (int i = 0; i < tileGroup.transform.childCount; i++) {
            if (i == tileGroup.transform.childCount / 2)
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
        ResetUICardCount();
        reset = true;
    }

    public void PickEditBuilding() {
        if (cam == null)
            return;

        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null) {

            if (hit.collider.tag == "Building") {
                selectBuilding = hit.transform.gameObject;
                selectbuildingStatus = selectBuilding;

                if (selectBuilding.GetComponent<BuildingObject>().data.id > 990)
                    selectBuilding = null;

            }
            else if (hit.collider.tag == "Tile") {
                if (hit.transform.gameObject.transform.childCount != 0) {
                    selectBuilding = hit.transform.GetChild(0).gameObject;
                    selectbuildingStatus = selectBuilding;

                    if (selectBuilding.GetComponent<BuildingObject>().data.id > 990)
                        selectBuilding = null;
                }
                else {
                    gameObject.transform.GetChild(4).gameObject.SetActive(false);
                    return;
                }
            }

            if (selectBuilding != null) {
                selectBuilding.GetComponent<PolygonCollider2D>().enabled = false;
                startEditPosition = selectBuilding.transform.position;
            }
            ShowBuildingStatus();           
        }
        /*
        else
            gameObject.transform.GetChild(4).gameObject.SetActive(false);
            */
        
    }

    public void MoveEditBuilding() {
        
        if (selectBuilding == null)
            return;

        picking = true;
        cam.GetComponent<BitBenderGames.MobileTouchCamera>().enabled = false;        
        Vector3 mousePosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
        mousePosition.z = 0;

        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        

        if (hit.collider != null) {
            if (hit.collider.tag == "Tile") {
                targetTile = hit.transform.gameObject;
                Vector3 buildingPosition = targetTile.transform.position;
                buildingPosition.z = 0;
                selectBuilding.transform.position = buildingPosition;
                

            }
            else if (hit.collider.tag == "Building") {
                targetTile = hit.transform.parent.gameObject;
                Vector3 buildingPosition = targetTile.transform.position;
                buildingPosition.z = 0;
                selectBuilding.transform.position = buildingPosition;
            }
        }
        else {
            targetTile = null;
            selectBuilding.transform.position = mousePosition;
        }
    }

    public void DropEditBuilding() {
        if (selectBuilding == null)
            return;
        if(targetTile != null) {
            if (targetTile.GetComponent<TileObject>().buildingSet == false) {
                Vector3 position = targetTile.transform.position;
                position.z = 0;
                tileSetList[targetTile.GetComponent<TileObject>().tileNum] = tileSetList[selectBuilding.transform.parent.GetComponent<TileObject>().tileNum];
                selectBuilding.transform.parent.GetComponent<TileObject>().buildingSet = false;
                tileSetList[selectBuilding.transform.parent.GetComponent<TileObject>().tileNum] = 0;
                selectBuilding.transform.SetParent(targetTile.transform);
                selectBuilding.transform.position = position;
                targetTile.GetComponent<TileObject>().buildingSet = true;
            }
            else
                selectBuilding.transform.position = startEditPosition;
        }
        else {
            //selectBuilding.transform.position = startEditPosition;
            if (picking == true)
                DeleteBuilding();
            else
                selectBuilding.transform.position = startEditPosition;
        }

        picking = false;
        selectBuilding.GetComponent<PolygonCollider2D>().enabled = true;
        selectBuilding = null;
        cam.GetComponent<BitBenderGames.MobileTouchCamera>().enabled = true;        
    }

    public void DeleteBuilding() {
        if (selectbuildingStatus == null)
            return;

        if (selectbuildingStatus.GetComponent<BuildingObject>().data.id > 990)
            return;

        GameObject slot = FindCard(selectbuildingStatus.GetComponent<BuildingObject>().data.id);
        int count = BuildingCount(selectbuildingStatus);
        count--;
        slot.transform.GetChild(2).GetComponent<Text>().text = count.ToString() + " / " + selectbuildingStatus.GetComponent<BuildingObject>().data.card.placementLimit;

        tileSetList[selectbuildingStatus.transform.parent.GetComponent<TileObject>().tileNum] = 0;
        selectbuildingStatus.transform.parent.GetComponent<TileObject>().buildingSet = false;
        gameObject.transform.GetChild(4).gameObject.SetActive(false);
        Destroy(selectbuildingStatus);
    }

    public void ShowBuildingStatus() {
        if (selectbuildingStatus == null)
            return;

        gameObject.transform.GetChild(4).GetChild(0).GetComponent<Text>().text = selectbuildingStatus.GetComponent<BuildingObject>().data.card.name; // 이름부분 (canvas => buildingStatus => BuildingName)
        gameObject.transform.GetChild(4).GetChild(1).GetChild(1).GetComponent<Text>().text = selectbuildingStatus.GetComponent<BuildingObject>().data.card.hitPoint.ToString(); // 이름부분 (canvas => buildingStatus => 체력부분)
        gameObject.transform.GetChild(4).gameObject.SetActive(true);

    }

    public int BuildingCount(GameObject _object) {
        int count = 0;
        
        for(int i = 0; i < tileGroup.transform.childCount; i++) {
            if (tileGroup.transform.GetChild(i).childCount != 0) {
                GameObject compareObject = tileGroup.transform.GetChild(i).GetChild(0).gameObject;

                if (_object.GetComponent<BuildingObject>().data.id == compareObject.GetComponent<BuildingObject>().data.id)
                    count++;
            }
        }
        return count;
    }

    public void CheckUICardCount() {
        if (uicontent == null)
            return;

        for(int i = 0; i < uicontent.transform.childCount; i++) // 페이지 검사
        {
            for(int j = 0; j< uicontent.transform.GetChild(i).childCount; j++) //  i 페이지 안에 있는 slot 검사
            {
                GameObject slot = uicontent.transform.GetChild(i).GetChild(j).gameObject; // i페이지 안에 있는 j번째 카드
                slot.transform.GetChild(2).GetComponent<Text>().text = BuildingCount(slot.GetComponent<DragHandler>().setObject).ToString() + " / " + slot.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().data.card.placementLimit.ToString();
            }
        }
    }

    public void ResetUICardCount() {
        if (uicontent == null)
            return;

        for (int i = 0; i < uicontent.transform.childCount; i++) // 페이지 검사
        {
            for (int j = 0; j < uicontent.transform.GetChild(i).childCount; j++) //  i 페이지 안에 있는 slot 검사
            {
                GameObject slot = uicontent.transform.GetChild(i).GetChild(j).gameObject; // i페이지 안에 있는 j번째 카드
                slot.transform.GetChild(2).GetComponent<Text>().text = 0 + " / " + slot.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().data.card.placementLimit.ToString();
            }
        }
    }

    public GameObject FindCard(int id) {

        GameObject card;

        if(uicontent != null) {
            for(int i = 0; i< uicontent.transform.childCount; i++) {
                for(int j = 0; j < uicontent.transform.GetChild(i).childCount; j++) {
                    if (uicontent.transform.GetChild(i).GetChild(j).GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().data.id == id) {
                        card = uicontent.transform.GetChild(i).GetChild(j).gameObject;
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

    public void ResetAllSliderValues() {
        for(int i=0; i<sliders.Length; i++) {
            sliders[i].value = 0;
        }
    }

    public void OnSliderValueChanged(GameObject slider) {
        slider.transform.Find("Text").GetComponent<Text>().text = slider.GetComponent<Slider>().value.ToString();
    }
}
