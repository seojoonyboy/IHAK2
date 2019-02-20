using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class MenuSceneController : MonoBehaviour {

    public enum Windows {
        BASIC = 0,
        DECKLIST = 1,
        SHOP = 2
    }

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.MenuScene;

    [SerializeField] Transform buttonSelect;
    [SerializeField] Transform buttonList;
    [SerializeField] GameObject switchButtons;
    [SerializeField] Text userNickname;
    [SerializeField] GameObject leaderDeck;
    [SerializeField] DeckListController deckListController;

    private HorizontalScrollSnap hss;
    private Windows openedWindow;
    private static int pageNum = 2;

    MenuSceneEventHandler eventHandler;

    private void Awake() {
        eventHandler = MenuSceneEventHandler.Instance;
        eventHandler.AddListener(MenuSceneEventHandler.EVENT_TYPE.INITIALIZE_DECK_FINISHED, SetLeaderTileGroup);
        eventHandler.AddListener(MenuSceneEventHandler.EVENT_TYPE.SET_LEADER_DECK_TOUCH_POWER, SetLeaderDeckTouchPower);
    }

    private void SetLeaderDeckTouchPower(Enum Event_Type, Component Sender, object Param) {
        if(Param != null) {
            GameObject go = AccountManager.Instance.transform.GetChild(0).GetChild(AccountManager.Instance.leaderIndex).gameObject;

            DataModules.ProductResources productResources = (DataModules.ProductResources)Param;
            go.GetComponent<TileGroup>().touchPerProdPower = productResources;
            leaderDeck.transform.GetChild(0).transform.GetComponent<TileGroup>().touchPerProdPower = productResources;
        }
    }

    private void OnDestroy() {
        eventHandler.RemoveListener(MenuSceneEventHandler.EVENT_TYPE.INITIALIZE_DECK_FINISHED, SetLeaderTileGroup);
        eventHandler.RemoveListener(MenuSceneEventHandler.EVENT_TYPE.SET_LEADER_DECK_TOUCH_POWER, SetLeaderDeckTouchPower);
    }

    private void SetLeaderTileGroup(Enum Event_Type, Component Sender, object Param) {
        foreach(Transform tf in leaderDeck.transform) {
            Destroy(tf.gameObject);
        }
        GameObject go = AccountManager.Instance.transform.GetChild(0).GetChild((int)Param).gameObject;
        go.SetActive(true);

        GameObject lo = Instantiate(go, leaderDeck.transform);

        foreach(Transform tile in lo.transform) {
            if(tile.childCount > 1) SetSpineAnimation(tile.GetChild(0));
        }
        go.SetActive(false);
    }

    private void SetSpineAnimation(Transform aniTransform) {
        Spine.Unity.SkeletonAnimation ani = aniTransform.GetComponent<Spine.Unity.SkeletonAnimation>();
        if(ani == null) return;
        ani.enabled = true;
        ani.GetComponent<TileSpineAnimation>().enabled = true;
    }
    

    // Use this for initialization
    void Start() {
        openedWindow = Windows.BASIC;
        hss = FindObjectOfType<HorizontalScrollSnap>();
        userNickname.text = AccountManager.Instance.userInfos.nickname;
        clickMenuButton(pageNum);
        //MenuSceneEventHandler.Instance.PostNotification(MenuSceneEventHandler.EVENT_TYPE.SET_TILE_OBJECTS_COMPLETED, null);
    }

    public void switchButton() {
        iTween.MoveTo(buttonSelect.GetChild(1).gameObject, iTween.Hash("x", buttonList.GetChild(hss.CurrentPage).position.x, "time", 0.3f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
        pageNum = hss.CurrentPage;
        StartCoroutine(HideButton());
    }

    IEnumerator HideButton() {
        yield return new WaitForSeconds(0.2f);
        switch (pageNum) {
            case 0:
                buttonSelect.GetChild(1).GetChild(0).gameObject.SetActive(false); // 왼쪽 화살표 (MenuCanvas/MainButtons/SelectedButton/ -> 0번째 자식)
                buttonSelect.GetChild(1).GetChild(1).gameObject.SetActive(true); // 왼쪽 화살표 (MenuCanvas/MainButtons/SelectedButton/ -> 1번째 자식)
                break;
            case 3:
                buttonSelect.GetChild(1).GetChild(0).gameObject.SetActive(true);
                buttonSelect.GetChild(1).GetChild(1).gameObject.SetActive(false);
                break;
            default :
                buttonSelect.GetChild(1).GetChild(0).gameObject.SetActive(true);
                buttonSelect.GetChild(1).GetChild(1).gameObject.SetActive(true);
                break;
        }
    }

    public void clickMenuButton(int page) {
        hss.GoToScreen(page);
        switchButton();
    }

    public void OpenOption() {
        OptionController oc = FindObjectOfType<OptionController>();
        oc.EnterOptionWindow(true);
    }
}
