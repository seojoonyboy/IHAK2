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
    [SerializeField] Transform produceButonList;
    [SerializeField] Text playerName;
    [SerializeField] GameObject playerCity;
    [SerializeField] Transform cityPos;
    

    private HorizontalScrollSnap hss;
    private GameObject city;

    private void Awake() {
        GameObject go = AccountManager.Instance.transform.GetChild(0).GetChild(AccountManager.Instance.leaderIndex).gameObject;
        go.SetActive(true);
        GameObject ld = (GameObject)Instantiate(go, playerCity.transform);
        ld.transform.localScale = new Vector3(1920 / (float)Screen.height, 1920 / (float)Screen.height, 1);
        go.SetActive(false);
    }

    // Use this for initialization
    void Start() {
        playerName.text = AccountManager.Instance.userInfos.nickname;
        hss = transform.GetChild(0).GetComponent<HorizontalScrollSnap>();
        playerCity.transform.GetChild(1).position = cityPos.position;
        IngameEnemyGenerator ieg = FindObjectOfType<IngameEnemyGenerator>();
        ieg.enemyTileGroup.transform.localPosition = playerCity.transform.GetChild(1).localPosition;
    }

    private void Update() {
    }

    public void SwitchCommand() {
        if (hss.CurrentPage != 0) {
            //iTween.MoveTo(commandBar, iTween.Hash("y", commandBarPos.GetChild(1).position.y, "time", 0.2f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
            ShutProductButtons(true);
            commandBar.transform.GetChild(0).gameObject.SetActive(false);
        }
        else {
            //iTween.MoveTo(commandBar, iTween.Hash("y", commandBarPos.GetChild(0).position.y, "time", 0.2f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
            ShutProductButtons(false);
            commandBar.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    private void ShutProductButtons(bool shut) {
        if(shut) {
            Debug.Log("shut");            
            Color shutColor = new Color(255, 255, 255, 0.7f);
            produceButonList.GetComponent<Image>().color = shutColor;
            for (int i = 0; i < 4; i++)
                produceButonList.GetChild(i).GetComponent<Image>().color = shutColor;
            produceButonList.GetChild(4).gameObject.SetActive(true);
        }
        else {
            Color onColor = new Color(255, 255, 255, 1.0f);
            produceButonList.GetComponent<Image>().color = onColor;
            for (int i = 0; i < 4; i++)
                produceButonList.GetChild(i).GetComponent<Image>().color = onColor;
            produceButonList.GetChild(4).gameObject.SetActive(false);
        }
    }

    public void ClickOption() {
        Modal.instantiate("게임에서 나가시겠습니까?", Modal.Type.YESNO, () => {
            GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
            gsm.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
            IngameScoreManager.Instance.DestroySelf();
        });
    }
}