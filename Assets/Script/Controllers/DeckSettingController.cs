using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataModules;
using System;
using UniRx;
using UniRx.Triggers;

public class DeckSettingController : MonoBehaviour {
    GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.DeckSettingScene;
    GameSceneManager gsm;
    PlayerInfosManager playerInfosManager;

    [SerializeField] public GameObject deckSet;
    [SerializeField] private GameObject togglePref;
    [SerializeField] private Button chooseSpeciesBtn;
    [SerializeField] private Button speciesConfirmBtn;
    [SerializeField] private GameObject modal;
    [SerializeField] Sprite[] speciesPortraits;

    public Text 
        modalHeader,
        content;

    private int speciesId = 0;
    public int SpeciesId {
        get {
            return speciesId;
        }
    }

    private void Start() {
        playerInfosManager = PlayerInfosManager.Instance;
        gsm = FindObjectOfType<GameSceneManager>();
        deckSet = GameObject.FindGameObjectWithTag("TileGroup");

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
        int length = Enum.GetValues(typeof(Species.Type)).Length;
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
        deck.Id = playerInfosManager.decks.Capacity;
        deck.deckData = transform.GetChild(0).GetChild(0).GetComponent<DropHandler>().deckData; // 타일 하위 오브젝트 스크립트 말고 그룹용 오브젝트 스크립트를 만들어야하나?
        deck.species = (Species.Type)speciesId;
        deck.Name = inputText;

        playerInfosManager.AddDeck(deck);

        Destroy(deckSet);
        gsm.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
    }

    public void returnButton() {
        Destroy(deckSet);
        gsm.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
    }
}
