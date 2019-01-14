using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class IngameSceneUIController : MonoBehaviour {

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.IngameScene;

    [SerializeField] Transform territoryList;
    [SerializeField] Transform camera;
    [SerializeField] Text playerName;

    private float mousDownPosition;

    // Use this for initialization
    void Start() {
        playerName.text = AccountManager.Instance.userInfos.nickname;
        var downStream = this.gameObject.UpdateAsObservable().Where(_ => Input.GetMouseButtonDown(0)).Select(_ => mousDownPosition = Input.mousePosition.x);
        var upStream = this.gameObject.UpdateAsObservable().Where(_ => Input.GetMouseButtonUp(0)).Select(_ => Input.mousePosition.x);
        var dragStream = this.gameObject.UpdateAsObservable().SkipUntil(downStream).TakeUntil(upStream).RepeatUntilDestroy(this);
        dragStream.Where(_ => mousDownPosition - Input.mousePosition.x < -500).ThrottleFirst(TimeSpan.FromMilliseconds(450)).Subscribe(_ => SwitchTerritory(true));
        dragStream.Where(_ => mousDownPosition - Input.mousePosition.x > 500).ThrottleFirst(TimeSpan.FromMilliseconds(450)).Subscribe(_ => SwitchTerritory(false));
    }


    void SwitchTerritory(bool left) {
        if (left && territoryList.localPosition.x < -1080) {
            iTween.MoveTo(territoryList.gameObject, iTween.Hash("x", territoryList.position.x + Screen.width, "time", 0.4f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
        }
        else if (!left && territoryList.localPosition.x > -1) {
            iTween.MoveTo(territoryList.gameObject, iTween.Hash("x", territoryList.position.x - Screen.width, "time", 0.4f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
        }

    }

    public void ClickOption() {
        Modal.instantiate("게임에서 나가시겠습니까?", Modal.Type.YESNO, () => {
            GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
            gsm.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
        });
    }
}