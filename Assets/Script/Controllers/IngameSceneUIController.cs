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
    [SerializeField] Transform lookingCity;
    [SerializeField] Transform switchBtn;
    [SerializeField] Transform playerRankBtn;
    [SerializeField] Transform dummyRankBtn;


    private HorizontalScrollSnap hss;
    private GameObject city;

    private void Awake() {
        GameObject go = AccountManager.Instance.transform.GetChild(0).GetChild(AccountManager.Instance.leaderIndex).gameObject;
        go.SetActive(true);
        GameObject ld = (GameObject)Instantiate(go, playerCity.transform);
        ld.transform.localScale = new Vector3(1920 / (float)Screen.height, 1920 / (float)Screen.height, 1);
        playerCity.GetComponent<IngameCityManager>().eachPlayersTileGroups.Add(ld);
        go.SetActive(false);
    }

    // Use this for initialization
    void Start() {
        playerName.text = AccountManager.Instance.userInfos.nickname;
        hss = transform.GetChild(0).GetComponent<HorizontalScrollSnap>();
        playerCity.transform.GetChild(1).position = cityPos.position;
        IngameEnemyGenerator ieg = FindObjectOfType<IngameEnemyGenerator>();
        ieg.tileGroup.transform.localPosition = playerCity.transform.GetChild(1).localPosition;
        lookingCity.GetChild(hss.CurrentPage).localScale = new Vector3(1.5f, 1.5f, 1);
        switchBtn.GetChild(0).gameObject.SetActive(false);
    }

    public void SwitchCommand() {
        
        if (hss.CurrentPage != 0) {
            ShutProductButtons(true);
            commandBar.transform.GetChild(0).gameObject.SetActive(false);
            playerRankBtn.GetChild(0).gameObject.SetActive(false);
        }
        else {
            ShutProductButtons(false);
            commandBar.transform.GetChild(0).gameObject.SetActive(true);
            dummyRankBtn.GetChild(0).gameObject.SetActive(false);
        }
        
        StartCoroutine(HideButton());
    }

    private void ShutProductButtons(bool shut) {
        if(shut) {
            Debug.Log("shut");
            playerCity.GetComponent<IngameCityManager>().CurrentView = 1;
            Color shutColor = new Color(255, 255, 255, 0.7f);
            produceButonList.GetComponent<Image>().color = shutColor;
            for (int i = 0; i < 4; i++)
                produceButonList.GetChild(i).GetComponent<Image>().color = shutColor;
            produceButonList.GetChild(4).gameObject.SetActive(true);
        }
        else {
            playerCity.GetComponent<IngameCityManager>().CurrentView = 0;
            Color onColor = new Color(255, 255, 255, 1.0f);
            produceButonList.GetComponent<Image>().color = onColor;
            for (int i = 0; i < 4; i++)
                produceButonList.GetChild(i).GetComponent<Image>().color = onColor;
            produceButonList.GetChild(4).gameObject.SetActive(false);
        }
    }

    public void SwtichCity(bool left) {
        if(left) {
            hss.GoToScreen(hss.CurrentPage - 1);
        }
        if(!left) {
            hss.GoToScreen(hss.CurrentPage + 1);
        }
        SwitchCommand();
    }

    IEnumerator HideButton() {
        yield return new WaitForSeconds(0.2f);
        lookingCity.GetChild(hss._previousPage).localScale = new Vector3(1.0f, 1.0f, 1);
        lookingCity.GetChild(hss.CurrentPage).localScale = new Vector3(1.5f, 1.5f, 1);
        switch (hss.CurrentPage) {
            case 0:
                switchBtn.GetChild(0).gameObject.SetActive(false);
                switchBtn.GetChild(1).gameObject.SetActive(true);
                break;
            default:
                switchBtn.GetChild(0).gameObject.SetActive(true);
                switchBtn.GetChild(1).gameObject.SetActive(false);
                break;
        }
    }

    public void PopRank(bool player) {
        if (player) {
            if (playerRankBtn.GetChild(0).gameObject.activeSelf)
                playerRankBtn.GetChild(0).gameObject.SetActive(false);
            else
                playerRankBtn.GetChild(0).gameObject.SetActive(true);
        }
        else {
            if (dummyRankBtn.GetChild(0).gameObject.activeSelf)
                dummyRankBtn.GetChild(0).gameObject.SetActive(false);
            else
                dummyRankBtn.GetChild(0).gameObject.SetActive(true);
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