using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class MenuSceneController2 : MonoBehaviour {

    public enum Windows {
        BASIC = 0,
        DECKLIST = 1,
        SHOP = 2
    }

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.MenuScene;

    //[SerializeField] Transform buttonSelect;
    [SerializeField] Transform buttonList;
    [SerializeField] GameObject deckListWnd;
    [SerializeField] Text userNickname;
    [SerializeField] DeckListController deckListController;
    [SerializeField] Button exitDeckList;

    private HorizontalScrollSnap hss;
    private Windows openedWindow;
    private static int pageNum = 2;
    private bool movingSelect = false;

    MenuSceneEventHandler eventHandler;


    private void Awake() {
        eventHandler = MenuSceneEventHandler.Instance;
        eventHandler.AddListener(MenuSceneEventHandler.EVENT_TYPE.INITIALIZE_DECK_FINISHED, SetLeaderTileGroup);
        eventHandler.AddListener(MenuSceneEventHandler.EVENT_TYPE.SET_LEADER_DECK_TOUCH_POWER, SetLeaderDeckTouchPower);
    }

    private void SetLeaderDeckTouchPower(Enum Event_Type, Component Sender, object Param) {
        if (Param != null) {
            GameObject go = AccountManager.Instance.transform.GetChild(0).GetChild(AccountManager.Instance.leaderIndex).gameObject;

            DataModules.ProductResources productResources = (DataModules.ProductResources)Param;
            go.GetComponent<TileGroup>().touchPerProdPower = productResources;
        }
    }

    private void OnDestroy() {
        eventHandler.RemoveListener(MenuSceneEventHandler.EVENT_TYPE.INITIALIZE_DECK_FINISHED, SetLeaderTileGroup);
        eventHandler.RemoveListener(MenuSceneEventHandler.EVENT_TYPE.SET_LEADER_DECK_TOUCH_POWER, SetLeaderDeckTouchPower);
    }

    private void SetLeaderTileGroup(Enum Event_Type, Component Sender, object Param) {
        //foreach(Transform tf in leaderDeck.transform) {
        //    Destroy(tf.gameObject);
        //}
        //GameObject go = AccountManager.Instance.transform.GetChild(0).GetChild((int)Param).gameObject;
        //go.SetActive(true);

        //GameObject lo = Instantiate(go, leaderDeck.transform);

        //foreach(Transform tile in lo.transform) {
        //    if(tile.childCount > 1) SetSpineAnimation(tile.GetChild(0));
        //}
        //Transform background = lo.transform.Find("Background");
        //foreach(Transform bg in background) {
        //    if (bg.name == "Dissolve") continue;
        //    bg.gameObject.SetActive(false);
        //}
        //go.SetActive(false);
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
        buttonList.GetChild(0).GetComponent<Button>().OnClickAsObservable().Subscribe(_ => deckListWnd.SetActive(true));
        exitDeckList.OnClickAsObservable().Subscribe(_ => deckListWnd.SetActive(false));
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
}
