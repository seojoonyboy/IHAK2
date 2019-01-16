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

    public Text 
        modalHeader,
        content;

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
        playerInfosManager.transform.GetChild(0).GetChild(playerInfosManager.selectNumber).gameObject.SetActive(true);
        tileGroup = playerInfosManager.transform.gameObject.transform.GetChild(0).GetChild(playerInfosManager.selectNumber).gameObject;
        deckListController = FindObjectOfType<DeckListController>();
        TilebuildingList();

        resetButton.OnClickAsObservable().Subscribe(_ => resetTile());
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
        if (prevData != null) {
            Debug.Log("Deck 수정 버튼을 통한 접근");
            InitPrevData();
        }
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

    private void InitPrevData() {

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
        Modal.instantiate("덱 이름 설정", "덱 이름을 입력해주세요", Modal.Type.INSERT, Callback);
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
            //for (int i = 0; i < tileSetList.Count; i++)
            //    playerInfosManager.decks[playerInfosManager.selectNumber].coordsSerial[i] = tileSetList[i];
            deck.id = prevData.id;
            playerInfosManager.ModifyDeck(deck);
            //playerInfosManager.decks[playerInfosManager.selectNumber].name = inputText;
        }

        /*
        GameObject go = GameObject.Find("TileGroup(Clone)");
        PrefabUtility.CreatePrefab("Assets/Resources/Prefabs/LeaderDeck.prefab", go);
        */
        prevData = null;
        tileGroup.SetActive(false);
        gsm.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
    }

    public void returnButton() {
        prevData = null;
        tileGroup.SetActive(false);
        gsm.startScene(sceneState, GameSceneManager.SceneState.MenuScene);

        //UnityEditor.PrefabUtility.CreatePrefab()
    }

    public void TilebuildingList() {
        /*
        if (playerInfosManager.decks[playerInfosManager.selectNumber] == null) {
            for (int i = 0; i < tileGroup.transform.childCount; i++) {
                tileSetList.Add(0);
            }
        }
        else if(playerInfosManager.decks[playerInfosManager.selectNumber].coordsSerial != null) {
            for (int i = 0; i < tileGroup.transform.childCount; i++) {
                tileSetList.Add(playerInfosManager.decks[playerInfosManager.selectNumber].coordsSerial[i]);
            }
        }
        */

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

}
