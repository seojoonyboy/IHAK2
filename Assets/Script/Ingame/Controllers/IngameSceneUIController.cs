using DataModules;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class IngameSceneUIController : MonoBehaviour {

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.IngameScene;

    [SerializeField] Text playerName;
    [SerializeField] GameObject playerCity;
    [SerializeField] GameObject enemyCity;
    [SerializeField] Transform playerRankBtn;
    [SerializeField] Transform dummyRankBtn;
    [SerializeField] Text ingameTimer;
    [SerializeField] IngameResultManager resultManager;
    [SerializeField] public Transform attackCard;
    [SerializeField] IngameHpSystem IngameHpSystem;

    public bool canPlaying = false;
    public bool canEnemyPlaying = false;

    private float time = 300;
    [SerializeField] public GameObject playerController;
    [SerializeField] public GameObject enemyController;

    IngameSceneEventHandler eventHandler;

    public static int deckId;

    void Awake() {
        eventHandler = IngameSceneEventHandler.Instance;
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, OnMyBuildingsAdded);
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.ENEMY_BUILDINGS_INFO_ADDED, OnEnemyBuildingsAdded);
    }

    void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, OnMyBuildingsAdded);
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.ENEMY_BUILDINGS_INFO_ADDED, OnEnemyBuildingsAdded);
    }

    private void OnEnemyBuildingsAdded(Enum Event_Type, Component Sender, object Param) {
        canEnemyPlaying = true;
    }

    private void OnMyBuildingsAdded(Enum Event_Type, Component Sender, object Param) {
        canPlaying = true;
    }

    // Use this for initialization
    void Start() {
        playerName.text = AccountManager.Instance.userInfos.nickname;
        dummyRankBtn.parent.GetComponent<Text>().text = "Dummy";
    }

    private void Update() {
        if (canPlaying && canEnemyPlaying) {
            //if (IngameHpSystem.enemyHQ.hp <= 0) {
            //    canPlaying = false;
            //    resultManager.GameOverWindow(IngameResultManager.GameOverType.WIN);
            //}
            //if (IngameHpSystem.playerHQ.hp <= 0) {
            //    canPlaying = false;
            //    resultManager.GameOverWindow(IngameResultManager.GameOverType.LOSE);
            //}
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
        Modal.instantiate("게임에서 나가시겠습니까?", Modal.Type.YESNO, ExitIngameScene);
    }

    public void ExitIngameScene() {
        GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
        gsm.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
        IngameScoreManager.Instance.DestroySelf();
    }

    IEnumerator EnemyRepair() {
        while(time > 60 && canEnemyPlaying == true) {
            yield return new WaitForSeconds(60f);
            IngameAlarm.instance.SetAlarm("Dummy 도시의 건물이 재건됩니다!!");
        }
    }
}