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

    public bool isPlaying = true;
    private float time = 300;
    [SerializeField] public GameObject playerController;
    [SerializeField] public GameObject enemyController;

    public static int deckId;

    private void OnDataCallback(HttpResponse response) {
        if (response.responseCode == 200) {
            if(response.data != null) {
                DeckDetail deckDetail = JsonReader.Read<DeckDetail>(response.data.ToString());

                ProductResources touchPower = deckDetail.productResources;
                FindObjectOfType<IngameCityManager>().productResources = touchPower;
            }
        }
    }

    private bool isDataInited(ProductResources touchProdPower) {
        Resource food = touchProdPower.food;
        Resource gold = touchProdPower.gold;
        Resource env = touchProdPower.env;

        if (food.food == 0 && food.gold == 0 && food.environment == 0) {
            if (gold.food == 0 && gold.gold == 0 && gold.environment == 0) {
                if (env.food == 0 && env.gold == 0 && env.environment == 0) {
                    return false;
                }
            }
        }
        return true;
    }

    // Use this for initialization
    void Start() {
        //IngameEnemyGenerator ieg = FindObjectOfType<IngameEnemyGenerator>();
        playerName.text = AccountManager.Instance.userInfos.nickname;
        dummyRankBtn.parent.GetComponent<Text>().text = "Dummy";
        //StartCoroutine("EnemyRepair");
        //enemyHQ = ieg.ingameCityManager.enemyHQ;
        //playerHQ = ieg.ingameCityManager.playerHQ;
    }

    private void Update() {
        if (isPlaying) {
            /*
            if (IngameScoreManager.Instance.playerScore > IngameScoreManager.Instance.dummyScore) {
                isPlaying = false;
                resultManager.GameOverWindow(IngameResultManager.GameOverType.WIN);
            }
            */
            if (enemyController.GetComponent<Container.PlayerResource>().hp < 1) {
                isPlaying = false;
                resultManager.GameOverWindow(IngameResultManager.GameOverType.WIN);
            }
            if (playerController.GetComponent<Container.PlayerResource>().hp < 1) {
                isPlaying = false;
                resultManager.GameOverWindow(IngameResultManager.GameOverType.LOSE);
            }
            
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
        while(time > 60 && isPlaying == true) {
            yield return new WaitForSeconds(60f);
            IngameAlarm.instance.SetAlarm("Dummy 도시의 건물이 재건됩니다!!");
        }
    }
}