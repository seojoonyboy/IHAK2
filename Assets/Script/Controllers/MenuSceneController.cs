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

    [SerializeField] Transform buttonList;
    [SerializeField] Transform windowList;
    [SerializeField] GameObject switchButtons;
    [SerializeField] Sprite selectedButton;
    [SerializeField] Sprite unSelectecButton;
    [SerializeField] Text userNickname;
    [SerializeField] GameObject leaderDeck;

    private HorizontalScrollSnap hss;
    private Windows openedWindow;
    private float mousDownPosition;
    private int selectedPosition;
    private Vector3[] buttonPos = { new Vector3(-360, 0, 0), new Vector3(0, 0, 0), new Vector3(360, 0, 0) };

    private void Awake() {
        
    }

    // Use this for initialization
    void Start() {
        openedWindow = Windows.BASIC;
        selectedPosition = 1;
        hss = FindObjectOfType<HorizontalScrollSnap>();
        userNickname.text = AccountManager.Instance.userInfos.nickname;        
        GameObject go  = AccountManager.Instance.transform.GetChild(0).GetChild(0).gameObject;
        go.SetActive(true);
        GameObject ld = (GameObject)Instantiate(go, leaderDeck.transform);
        go.SetActive(false);
    }

    public void switchButton() {
        iTween.MoveTo(buttonList.GetChild(1).gameObject, iTween.Hash("x", buttonPos[hss.CurrentPage].x, "time", 0.2f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
    }

    public void clickMenuButton(int page) {
        hss.GoToScreen(page);
        switchButton();
    }
}
