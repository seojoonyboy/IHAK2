using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class EditScenePanel : MonoBehaviour {

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.DeckSettingScene;

    [SerializeField] Transform content;
    [SerializeField] Transform leftBtn;
    [SerializeField] Transform rightBtn;
    [SerializeField] Text pageText;
    public int page;

    private float mouseDownPosition;
    

    private void Start() {
        page = 0;

        leftBtn.GetComponent<Button>().OnClickAsObservable().ThrottleFirst(TimeSpan.FromMilliseconds(500)).Subscribe(_ => switchButton(true));
        rightBtn.GetComponent<Button>().OnClickAsObservable().ThrottleFirst(TimeSpan.FromMilliseconds(500)).Subscribe(_ => switchButton(false));

        var downStream = content.gameObject.UpdateAsObservable().Where(_ => Input.GetMouseButtonDown(0)).Select(_ => mouseDownPosition = Input.mousePosition.x);
        var upStream = content.gameObject.UpdateAsObservable().Where(_ => Input.GetMouseButtonUp(0)).Select(_ => Input.mousePosition.x);
        var dragStream = content.gameObject.UpdateAsObservable().SkipUntil(downStream).TakeUntil(upStream).RepeatUntilDestroy(this);

        //dragStream.Where(_=> mouseDownPosition - Input.mousePosition.x < -500).ThrottleFirst(TimeSpan.FromMilliseconds(500)).Subscribe(_=> switchButton(true));
        //dragStream.Where(_ => mouseDownPosition - Input.mousePosition.x > 500).ThrottleFirst(TimeSpan.FromMilliseconds(500)).Subscribe(_ => switchButton(false));

    }

    public void switchButton(bool left) {
        if (left) {
            if (page == 0)
                return;

            for (int i = 0; i < content.childCount; i++) 
                iTween.MoveTo(content.GetChild(i).gameObject, iTween.Hash("x", content.GetChild(i).position.x + Screen.width, "time", 0.4f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));

            page--;
        }
        else {
            if (page == content.childCount - 1)
                return;

            for (int i = 0; i < content.childCount; i++) 
                iTween.MoveTo(content.GetChild(i).gameObject, iTween.Hash("x", content.GetChild(i).position.x - Screen.width, "time", 0.4f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));

            page++;
        }
        pageText.text = (page + 1) + " / " + content.childCount;
    }
}
