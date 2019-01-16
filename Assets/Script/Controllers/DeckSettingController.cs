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
    Camera cam;
    [SerializeField] public GameObject tileGroup;
    [SerializeField] private GameObject togglePref;
    [SerializeField] private Button chooseSpeciesBtn;
    [SerializeField] private Button speciesConfirmBtn;
    [SerializeField] private GameObject modal;
    [SerializeField] Sprite[] speciesPortraits;
    [SerializeField] DeckListController deckListController;
    [SerializeField] public List<int> tileSetList;
    [SerializeField] public Button resetButton;
    [SerializeField] public bool modify;
    [SerializeField] public GameObject selectBuilding;
    [SerializeField] public GameObject targetTile;
    [SerializeField] public Vector3 startEditPosition;
    public Text 
        modalHeader,
        content;

    [Header("UISlider")]
    public Slider[] sliders;

    private int speciesId = 0;
    public int SpeciesId {
        get {
            return speciesId;
        }
    }

    public static Deck prevData = null;

    private void Start() {
        playerInfosManager = AccountManager.Instance;
        gsm = FindObjectOfType<GameSceneManager>();
        cam = Camera.main;
        var downStream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));
        var dragStream = Observable.EveryUpdate().Where(_ => Input.GetMouseButton(0));
        var upStream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonUp(0));

        playerInfosManager.transform.GetChild(0).GetChild(playerInfosManager.selectNumber).gameObject.SetActive(true);
        tileGroup = playerInfosManager.transform.gameObject.transform.GetChild(0).GetChild(playerInfosManager.selectNumber).gameObject;
        deckListController = FindObjectOfType<DeckListController>();
        TilebuildingList();

        resetButton.OnClickAsObservable().Subscribe(_ => resetTile());

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
        InitToggles();
    }

    private void InitToggles() {
        int length = 1;
        for(int i=0; i<length; i++) {
            ToggleGroup toggleGroup = modal.transform.Find("InnerModal/Body/DataArea/ToggleGroup").GetComponent<ToggleGroup>();
            GameObject pref = Instantiate(togglePref, toggleGroup.transform);
            pref.name = "Toggle" + i;
            int id = i;
            Toggle toggle = pref.GetComponent<Toggle>();
            toggle.group = toggleGroup;
            if(i == 0) {
                toggle.isOn = true;
                Toggle(toggle, id);
            }
            toggle.onValueChanged.AddListener(delegate { Toggle(toggle, id); });
            Image image = pref.transform.Find("Background/Image").GetComponent<Image>();
            image.sprite = speciesPortraits[i];
        }
        AddPrepareToggle(1);
        AddPrepareToggle(2);
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

    private void Toggle(Toggle t, int id) {
        if (t.isOn) {
            SetModalText(id);
        }
    }

    private void SetModalText(int id) {
        string species = ((Species.Type)id).ToString();
        modalHeader.text = species;
        content.text = species + "에 대한 설명";
    }

    public void settingButton() {
        if (prevData == null) {
            Modal.instantiate("덱 이름 설정", "덱 이름을 입력해주세요", null, Modal.Type.INSERT, Callback);
        }
        else {
            Modal.instantiate("덱 이름 설정", null, prevData.name, Modal.Type.INSERT, Callback);
        }
    }

    private void Callback(string inputText) {
        Deck deck = new Deck();
        deck.race = ((Species.Type)speciesId).ToString();
        deck.name = inputText;
        deck.coordsSerial = new int[tileSetList.Count + 1];
        for (int i = 0; i < tileSetList.Count; i++)
            deck.coordsSerial[i] = tileSetList[i];

        if (prevData == null) {
            playerInfosManager.AddDeck(deck);
        }
        else {
            deck.id = prevData.id;
            playerInfosManager.ModifyDeck(deck);
        }
        prevData = null;
        tileGroup.SetActive(false);
        gsm.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
    }

    public void returnButton() {
        prevData = null;
        tileGroup.SetActive(false);
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

            if (tileGroup.transform.GetChild(i).childCount != 0)
                Destroy(tileGroup.transform.GetChild(i).GetChild(0).gameObject);
            tileGroup.transform.GetChild(i).GetComponent<TileObject>().buildingSet = false;
            tileSetList[i] = 0;
        }
    }

    public void PickEditBuilding() {
        if (cam == null)
            return;

        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if(hit.collider != null) {
            if (hit.collider.tag == "Building") {
                selectBuilding = hit.transform.gameObject;
            }
            else if (hit.collider.tag == "Tile") {
                if (hit.transform.gameObject.transform.childCount != 0)
                    selectBuilding = hit.transform.GetChild(0).gameObject;
                else
                    return;
            }
            selectBuilding.GetComponent<PolygonCollider2D>().enabled = false;
            startEditPosition = selectBuilding.transform.position;
        }
    }

    public void MoveEditBuilding() {
        if (selectBuilding == null)
            return;

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
            selectBuilding.transform.position = startEditPosition;
        }
        cam.GetComponent<BitBenderGames.MobileTouchCamera>().enabled = true;
        selectBuilding.GetComponent<PolygonCollider2D>().enabled = true;
        selectBuilding = null;
    }

    public int ChangeSliderValue(int value = 0, string type = null) {
        Slider slider = null;
        switch (type) {
            case "Eco":
                slider = sliders[0];
                break;
            case "Industry":
                slider = sliders[4];
                break;
            case "Shield":
                slider = sliders[2];
                break;
            case "Farm":
                slider = sliders[1];
                break;
        }
        if (slider != null) {
            slider.value += value;
            return (int)slider.value;
        }
        return 0;
    }
}
