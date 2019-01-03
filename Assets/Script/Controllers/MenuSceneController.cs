using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

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

    private Windows openedWindow;
    private float mousDownPosition;

    private void Awake() {
        
    }

    // Use this for initialization
    void Start() {
        openedWindow = Windows.BASIC;
        buttonList.GetChild(0).GetComponent<Image>().sprite = buttonList.GetChild(3).GetComponent<Image>().sprite;
        buttonList.GetChild(4).GetComponent<Image>().sprite = buttonList.GetChild(1).GetComponent<Image>().sprite;

        switchButtons.transform.GetChild(0).GetComponent<Button>().OnClickAsObservable().ThrottleFirst(TimeSpan.FromMilliseconds(420)).Subscribe(_ => switchButton(true));
        switchButtons.transform.GetChild(1).GetComponent<Button>().OnClickAsObservable().ThrottleFirst(TimeSpan.FromMilliseconds(420)).Subscribe(_ => switchButton(false));

        //var camera = Camera.main.gameObject;
        var downStream = windowList.gameObject.UpdateAsObservable().Where(_ => Input.GetMouseButtonDown(0)).Select(_ => mousDownPosition = Input.mousePosition.x);
        var upStream = windowList.gameObject.UpdateAsObservable().Where(_ => Input.GetMouseButtonUp(0)).Select(_ => Input.mousePosition.x);
        var dragStream = windowList.gameObject.UpdateAsObservable().SkipUntil(downStream).TakeUntil(upStream).RepeatUntilDestroy(this);
        dragStream.Where(_ => mousDownPosition - Input.mousePosition.x < -500).ThrottleFirst(TimeSpan.FromMilliseconds(420)).Subscribe(_ => switchButton(true));
        dragStream.Where(_ => mousDownPosition - Input.mousePosition.x > 500).ThrottleFirst(TimeSpan.FromMilliseconds(420)).Subscribe(_ => switchButton(false));
    }

    public void switchButton(bool left) {
        if (left) {
            for(int i = 0; i < 4; i++) {
                if(i < 2)
                    iTween.MoveTo(windowList.GetChild(i).gameObject, iTween.Hash("x", windowList.GetChild(i).position.x + Screen.width, "time", 0.4f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
                iTween.MoveTo(buttonList.GetChild(i).gameObject, iTween.Hash("x", buttonList.GetChild(i).position.x + Screen.width / 3, "time", 0.4f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
            }
            buttonList.GetChild(4).localPosition = new Vector3(-720, 0, 0);
            buttonList.GetChild(4).GetComponent<Image>().sprite = buttonList.GetChild(2).GetComponent<Image>().sprite;
            buttonList.GetChild(4).SetAsFirstSibling();
            windowList.GetChild(2).localPosition = new Vector3(-1080, 45, 0);
            windowList.GetChild(2).SetAsFirstSibling();
        }
        else {
            for (int i = 1; i < 5; i++) {
                if(i < 3)
                    iTween.MoveTo(windowList.GetChild(i).gameObject, iTween.Hash("x", windowList.GetChild(i).position.x - Screen.width, "time", 0.4f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
                iTween.MoveTo(buttonList.GetChild(i).gameObject, iTween.Hash("x", buttonList.GetChild(i).position.x - Screen.width / 3, "time", 0.4f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
            }
            buttonList.GetChild(0).localPosition = new Vector3(720, 0, 0);
            buttonList.GetChild(0).GetComponent<Image>().sprite = buttonList.GetChild(2).GetComponent<Image>().sprite;
            buttonList.GetChild(0).SetAsLastSibling();
            windowList.GetChild(0).localPosition = new Vector3(1080, 45, 0);
            windowList.GetChild(0).SetAsLastSibling();
        }
    }
}
