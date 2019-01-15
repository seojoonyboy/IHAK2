using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class IngameSceneUIController : MonoBehaviour {

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.IngameScene;

    [SerializeField] GameObject commandBar;
    [SerializeField] Transform commandBarPos;
    [SerializeField] Text playerName;
    [SerializeField] GameObject playerCity;

    private HorizontalScrollSnap hss;

    // Use this for initialization
    void Start() {
        playerName.text = AccountManager.Instance.userInfos.nickname;
        hss = transform.GetChild(0).GetComponent<HorizontalScrollSnap>();
        GameObject go = AccountManager.Instance.transform.GetChild(0).GetChild(0).gameObject;
        go.SetActive(true);
        GameObject ld = (GameObject)Instantiate(go, playerCity.transform);
        go.SetActive(false);
    }


    public void SwitchCommand() {
        if(hss.CurrentPage != 0)
            iTween.MoveTo(commandBar, iTween.Hash("y", commandBarPos.GetChild(1).position.y, "time", 0.2f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
        else
            iTween.MoveTo(commandBar, iTween.Hash("y", commandBarPos.GetChild(0).position.y, "time", 0.2f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
    }

    public void ClickOption() {
        Modal.instantiate("게임에서 나가시겠습니까?", Modal.Type.YESNO, () => {
            GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
            gsm.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
        });
    }
}