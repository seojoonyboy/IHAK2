using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class MenuController : MonoBehaviour {
    public enum Windows {
        BASIC = 0,
        DECKLIST = 1,
        SHOP = 2
    }

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.MenuScene;

    //[SerializeField] Transform buttonSelect;
    [SerializeField] Transform buttonList;
    [SerializeField] GameObject deckListWnd;
    [SerializeField] GameObject missionListWnd;
    [SerializeField] Text userNickname;
    [SerializeField] DeckListController deckListController;
    [SerializeField] Button exitDeckList;
    [SerializeField] AccountManager accountManager;
    [SerializeField] public GameObject town;
    [SerializeField] public GameObject onCanvasTownPos;

    private HorizontalScrollSnap hss;
    private Windows openedWindow;
    private static int pageNum = 2;
    private bool movingSelect = false;

    MenuSceneEventHandler eventHandler;


    private void Awake() {
        eventHandler = MenuSceneEventHandler.Instance;
        accountManager = AccountManager.Instance;
        eventHandler.AddListener(MenuSceneEventHandler.EVENT_TYPE.INITIALIZE_DECK_FINISHED, SetLeaderTileGroup);
        eventHandler.AddListener(MenuSceneEventHandler.EVENT_TYPE.CHANGE_LEADER_DECK, ChangeLeaderDeck);
        eventHandler.AddListener(MenuSceneEventHandler.EVENT_TYPE.GET_MISSION_DATA_FINISHED, MissionLoadComplete);
    }

    private void ChangeLeaderDeck(Enum Event_Type, Component Sender, object Param) {
        SetTown();
    }

    private void OnDestroy() {
        eventHandler.RemoveListener(MenuSceneEventHandler.EVENT_TYPE.INITIALIZE_DECK_FINISHED, SetLeaderTileGroup);
        eventHandler.RemoveListener(MenuSceneEventHandler.EVENT_TYPE.CHANGE_LEADER_DECK, ChangeLeaderDeck);
        eventHandler.RemoveListener(MenuSceneEventHandler.EVENT_TYPE.GET_MISSION_DATA_FINISHED, MissionLoadComplete);
    }

    private void SetLeaderTileGroup(Enum Event_Type, Component Sender, object Param) {
        SetTown();
    }

    private void SetSpineAnimation(Transform aniTransform) {
        Spine.Unity.SkeletonAnimation ani = aniTransform.GetComponent<Spine.Unity.SkeletonAnimation>();
        if (ani == null) return;
        ani.enabled = true;
        ani.GetComponent<TileSpineAnimation>().enabled = true;
    }


    // Use this for initialization
    void Start() {
        openedWindow = Windows.BASIC;
        hss = FindObjectOfType<HorizontalScrollSnap>();
        userNickname.text = AccountManager.Instance.userInfos.nickname;
        deckListWnd.SetActive(false);
        missionListWnd.SetActive(false);
        buttonList.GetChild(2).GetComponent<Button>().OnClickAsObservable().Subscribe(_ => OpenDeckWindow());
        exitDeckList.OnClickAsObservable().Subscribe(_ => CloseDeckWindow());

        //clickMenuButton(pageNum);
        //MenuSceneEventHandler.Instance.PostNotification(MenuSceneEventHandler.EVENT_TYPE.SET_TILE_OBJECTS_COMPLETED, null);
    }

    //public void switchButton() {
    //    iTween.MoveTo(selectArrow.gameObject, iTween.Hash("x", buttonList.GetChild(hss.CurrentPage).position.x, "time", 0.3f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
    //    iTween.MoveTo(buttonSelect.GetChild(1).gameObject, iTween.Hash("x", buttonList.GetChild(hss.CurrentPage).position.x, "time", 0.3f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
    //    if (pageNum != 3 && !movingSelect) {
    //        movingSelect = true;
    //        buttonList.GetChild(pageNum).GetChild(0).GetComponent<ButtonAniSet>().SetState(ButtonAniSet.ButtonState.Inactive); //이미지(버튼 아님)의 스켈레톤 애니메이션
    //    }
    //    pageNum = hss.CurrentPage;
    //    StartCoroutine(HideButton());
    //}

    //public void clickMenuButton(int page) {
    //    hss.GoToScreen(page);
    //    switchButton();
    //}

    public void OpenOption() {
        OptionController oc = FindObjectOfType<OptionController>();
        oc.EnterOptionWindow(true);
    }

    public void OpenDeckWindow() {
        deckListWnd.SetActive(true);
        town.SetActive(false);
    }

    public void CloseDeckWindow() {
        deckListWnd.SetActive(false);
        town.SetActive(true);
    }

    public void SetTown() {
        if (accountManager.decks.Count == 0) return;
        if (town.transform.childCount != 0) Destroy(town.transform.GetChild(0).gameObject);
        town.transform.position = onCanvasTownPos.transform.position;

        int LeaderNum = accountManager.leaderIndex;
        GameObject showTown = Instantiate(accountManager.transform.GetChild(0).GetChild(LeaderNum).gameObject, town.transform);
        showTown.transform.localPosition = Vector3.zero;
        showTown.transform.localScale = new Vector3(1, 1, 1);
        showTown.SetActive(true);
    }

    public void OpenMissionWindow() {
        town.SetActive(false);
        missionListWnd.SetActive(true);
    }

    public void CloseMissionWindow() {
        town.SetActive(true);
        missionListWnd.SetActive(false);
    }

    private void MissionLoadComplete(Enum Event_Type, Component Sender, object Param) {
        DataModules.MissionData missionData = (DataModules.MissionData)Param;
        GameSceneManager gsm = FindObjectOfType<GameSceneManager>();

        if (Param != null) {
            accountManager.mission = missionData;
            Debug.Log(accountManager.mission.title);
            gsm.startScene(sceneState, GameSceneManager.SceneState.IngameScene);
        }
    }


    public void StartIngame() {
        GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
        OpenMissionWindow();

        /*
        if (accountManager.decks.Count == 0)
            Modal.instantiate("저장된 덱정보가 없습니다.", Modal.Type.CHECK);
        else
            gsm.startScene(sceneState, GameSceneManager.SceneState.IngameScene);
        */
    }
}
