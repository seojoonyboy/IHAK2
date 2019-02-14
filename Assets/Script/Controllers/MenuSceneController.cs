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
    private static int pageNum = 1;

    MenuSceneEventHandler eventHandler;

    private void Awake() {
        eventHandler = MenuSceneEventHandler.Instance;
        eventHandler.AddListener(MenuSceneEventHandler.EVENT_TYPE.CHANGE_MAINSCENE_TILE_GROUP, ResetTileGroupFinished);
        eventHandler.AddListener(MenuSceneEventHandler.EVENT_TYPE.SET_LEADER_DECK_TOUCH_POWER, SetLeaderDeckTouchPower);

        eventHandler.PostNotification(MenuSceneEventHandler.EVENT_TYPE.CHANGE_MAINSCENE_TILE_GROUP, null, AccountManager.Instance.leaderIndex);
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
        eventHandler.RemoveListener(MenuSceneEventHandler.EVENT_TYPE.CHANGE_MAINSCENE_TILE_GROUP, ResetTileGroupFinished);
        eventHandler.RemoveListener(MenuSceneEventHandler.EVENT_TYPE.SET_LEADER_DECK_TOUCH_POWER, SetLeaderDeckTouchPower);
    }

    private void ResetTileGroupFinished(Enum Event_Type, Component Sender, object Param) {
        //TODO : 건물이 세개나 생성이 되는 이유를 알아야함
        foreach(Transform tf in leaderDeck.transform) {
            Destroy(tf.gameObject);
        }
        GameObject go = AccountManager.Instance.transform.GetChild(0).GetChild((int)Param).gameObject;
        go.SetActive(true);

        GameObject lo = Instantiate(go, leaderDeck.transform);

        foreach(Transform tile in lo.transform) {
            if(tile.childCount > 1) {
                SetSortingOrder(tile.GetChild(tile.childCount -1), GetSortingOrder(tile.GetChild(0)));
                for(int i= 0; i < tile.childCount - 1; i++) {
                    Destroy(tile.GetChild(i).gameObject);
                }
            }
        }
        go.SetActive(false);
    }

    private void SetSortingOrder(Transform tile, int order) {
        if(tile.GetComponent<SpriteRenderer>() != null) {
            tile.GetComponent<SpriteRenderer>().sortingOrder = order;
        }
        else {
            tile.GetComponent<MeshRenderer>().sortingOrder = order;
        }
    }

    private int GetSortingOrder(Transform tile) {
        if(tile.GetComponent<SpriteRenderer>() != null) {
            return tile.GetComponent<SpriteRenderer>().sortingOrder;
        }
        else {
            return tile.GetComponent<MeshRenderer>().sortingOrder;
        }
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
            
            case 2:
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
