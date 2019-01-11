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

    private Windows openedWindow;
    private float mousDownPosition;
    private int selectedPosition;

    private void Awake() {
        
    }

    // Use this for initialization
    void Start() {
        openedWindow = Windows.BASIC;
        selectedPosition = 1;
        //buttonList.GetChild(0).GetComponent<Image>().sprite = buttonList.GetChild(3).GetComponent<Image>().sprite;
        //buttonList.GetChild(4).GetComponent<Image>().sprite = buttonList.GetChild(1).GetComponent<Image>().sprite;

        //switchButtons.transform.GetChild(0).GetComponent<Button>().OnClickAsObservable().ThrottleFirst(TimeSpan.FromMilliseconds(420)).Subscribe(_ => switchButton(true));
        //switchButtons.transform.GetChild(1).GetComponent<Button>().OnClickAsObservable().ThrottleFirst(TimeSpan.FromMilliseconds(420)).Subscribe(_ => switchButton(false));

        //var downStream = this.gameObject.UpdateAsObservable().Where(_ => Input.GetMouseButtonDown(0)).Select(_ => mousDownPosition = Input.mousePosition.x);
        //var upStream = this.gameObject.UpdateAsObservable().Where(_ => Input.GetMouseButtonUp(0));
        //var dragStream = this.gameObject.UpdateAsObservable().SkipUntil(downStream).DistinctUntilChanged().Buffer(upStream).RepeatUntilDestroy(this);
        //dragStream.Where(_ => mousDownPosition - Input.mousePosition.x < -300).ThrottleFirst(TimeSpan.FromMilliseconds(420)).Subscribe(_ => switchButton(true));
        //dragStream.Where(_ => mousDownPosition - Input.mousePosition.x > 300).ThrottleFirst(TimeSpan.FromMilliseconds(420)).Subscribe(_ => switchButton(false));
        //GetComponent<HorizontalScrollSnap>()
    }

    public void switchButton(bool left) {
        if (left) {
            for (int i = 1; i < 5; i++) {
                iTween.MoveTo(buttonList.GetChild(i).gameObject, iTween.Hash("x", buttonList.GetChild(i).position.x - Screen.width / 3, "time", 0.4f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
            }
            for (int i = 0; i < 2; i++) {
                iTween.MoveTo(windowList.GetChild(i).gameObject, iTween.Hash("x", windowList.GetChild(i).position.x + Screen.width, "time", 0.4f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
            }
            buttonList.GetChild(0).localPosition = new Vector3(720, 0, 0);
            buttonList.GetChild(0).SetAsLastSibling();
            
            if (--selectedPosition < 0) selectedPosition = 2;
            if (selectedPosition == 0) {
                buttonList.GetChild(4).GetComponent<Image>().sprite = selectedButton;
                buttonList.GetChild(0).GetComponent<Image>().sprite = unSelectecButton;
            }
            if(selectedPosition == 1)
                buttonList.GetChild(4).GetComponent<Image>().sprite = unSelectecButton;
            windowList.GetChild(2).localPosition = new Vector3(-1080, 45, 0);
            windowList.GetChild(2).SetAsFirstSibling();
            
        }
        else {
            for (int i = 0; i < 4; i++) {
                iTween.MoveTo(buttonList.GetChild(i).gameObject, iTween.Hash("x", buttonList.GetChild(i).position.x + Screen.width / 3, "time", 0.4f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
            }
            for (int i = 1; i < 3; i++) {
                iTween.MoveTo(windowList.GetChild(i).gameObject, iTween.Hash("x", windowList.GetChild(i).position.x - Screen.width, "time", 0.4f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
            }
            buttonList.GetChild(4).localPosition = new Vector3(-720, 0, 0);
            buttonList.GetChild(4).SetAsFirstSibling();
            if (++selectedPosition > 2) selectedPosition = 0;
            if (selectedPosition == 2) {
                buttonList.GetChild(0).GetComponent<Image>().sprite = selectedButton;
                buttonList.GetChild(4).GetComponent<Image>().sprite = unSelectecButton;
            }
            if (selectedPosition == 1)
                buttonList.GetChild(0).GetComponent<Image>().sprite = unSelectecButton;
            windowList.GetChild(0).localPosition = new Vector3(1080, 45, 0);
            windowList.GetChild(0).SetAsLastSibling();
        }
    }
}
