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

    private HorizontalScrollSnap hss;
    private Windows openedWindow;
    private static int pageNum = 1;

    MenuSceneEventHandler eventHandler;

    private void Awake() {
        eventHandler = MenuSceneEventHandler.Instance;
        eventHandler.RemoveListener(MenuSceneEventHandler.EVENT_TYPE.CHANGE_MAINSCENE_TILE_GROUP, ResetTileGroupFinished);
        eventHandler.AddListener(MenuSceneEventHandler.EVENT_TYPE.CHANGE_MAINSCENE_TILE_GROUP, ResetTileGroupFinished);
    }

    private void ResetTileGroupFinished(Enum Event_Type, Component Sender, object Param) {
        if(leaderDeck == null) return;
        int num = leaderDeck.transform.childCount;
        if (num > 0) {
            for (int i = 0; i < num; i++)
                Destroy(leaderDeck.transform.GetChild(i).gameObject);
        }
        GameObject go = AccountManager.Instance.transform.GetChild(0).GetChild((int)Param).gameObject;
        go.SetActive(true);
        GameObject ld = (GameObject)Instantiate(go, leaderDeck.transform);
        go.SetActive(false);
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
