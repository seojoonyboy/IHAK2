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

    private void Awake() {
        
    }

    // Use this for initialization
    void Start() {
        openedWindow = Windows.BASIC;
        hss = FindObjectOfType<HorizontalScrollSnap>();
        userNickname.text = AccountManager.Instance.userInfos.nickname;        
        GameObject go  = AccountManager.Instance.transform.GetChild(0).GetChild(0).gameObject;
        go.SetActive(true);
        GameObject ld = (GameObject)Instantiate(go, leaderDeck.transform);
        go.SetActive(false);
        clickMenuButton(pageNum);
    }

    public void switchButton() {
        iTween.MoveTo(buttonSelect.GetChild(1).gameObject, iTween.Hash("x", buttonList.GetChild(hss.CurrentPage).position.x, "time", 0.2f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
        pageNum = hss.CurrentPage;
    }

    public void clickMenuButton(int page) {
        hss.GoToScreen(page);
        switchButton();
    }
}
