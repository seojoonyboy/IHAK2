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

    [SerializeField] GameObject commandBar;
    [SerializeField] Transform produceButonList;
    [SerializeField] Text playerName;
    [SerializeField] GameObject playerCity;
    [SerializeField] GameObject enemyCity;
    [SerializeField] Transform cityPos;
    [SerializeField] Transform lookingCity;
    [SerializeField] Transform switchBtn;
    [SerializeField] Transform playerRankBtn;
    [SerializeField] Transform dummyRankBtn;
    [SerializeField] Text ingameTimer;
    [SerializeField] IngameResultManager resultManager;
    [SerializeField] GameObject repairAlert;
    [SerializeField] Camera territoryCamera;
    [SerializeField] Text turn;
    [SerializeField] public Transform attackCard;
    

    private HorizontalScrollSnap hss;
    private GameObject city;
    private IngameEnemyGenerator ieg;
    public bool isPlaying = true;
    private float time = 300;
    private float baseCameraSize = 957.0f;
    private float baseScreenHeight = 1920.0f;
    private float screenRate;
    [SerializeField] IngameCityManager.BuildingInfo enemyHQ;
    [SerializeField] IngameCityManager.BuildingInfo playerHQ;

    public static int deckId;

    private void Awake() {
        GameObject go = AccountManager.Instance.transform.GetChild(0).GetChild(AccountManager.Instance.leaderIndex).gameObject;
        screenRate = baseScreenHeight / (Screen.height);
        enemyCity.transform.position = new Vector3(Screen.width, 0, 0);
        playerCity.transform.localScale = enemyCity.transform.localScale = playerCity.transform.localScale / 1080 * Screen.width;
        Camera.main.orthographicSize = territoryCamera.orthographicSize = baseCameraSize / screenRate;
        go.SetActive(true);
        GameObject ld = (GameObject)Instantiate(go, playerCity.transform);
        territoryCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        transform.GetChild(0).GetComponent<RawImage>().texture = territoryCamera.targetTexture;

        foreach(Transform tile in ld.transform) {
            tile.gameObject.layer = 8;
            foreach(Transform building in tile) {
                building.gameObject.layer = 8;
            }
        }
        ProductResources touchProdPower = ld.GetComponent<TileGroup>().touchPerProdPower;

        Transform target = playerCity.transform.GetChild(1).Find("Tile[2,2]");
        Vector2 hqPos = target.localPosition;
        enemyCity.GetComponent<IngameEnemyUnitGenerator>().SetLocations(new Vector2[4] {
            new Vector2(hqPos.x - 60, hqPos.y + 60),
            new Vector2(hqPos.x + 60, hqPos.y + 60),
            new Vector2(hqPos.x - 60, hqPos.y - 60),
            new Vector2(hqPos.x + 60, hqPos.y - 60)
        }, target.parent);

        ld.transform.localScale = new Vector3(1, 1, 1);
        playerCity.GetComponent<IngameCityManager>().eachPlayersTileGroups.Add(ld);
        go.SetActive(false);
    }

    private void OnDataCallback(HttpResponse response) {
        if (response.responseCode == 200) {
            if(response.data != null) {
                DeckDetail deckDetail = JsonReader.Read<DeckDetail>(response.data.ToString());

                ProductResources touchPower = deckDetail.productResources;
                playerCity.GetComponent<IngameCityManager>().productResources = touchPower;
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
        ieg = enemyCity.GetComponent<IngameEnemyGenerator>();
        playerName.text = AccountManager.Instance.userInfos.nickname;
        hss = transform.GetChild(1).GetComponent<HorizontalScrollSnap>();
        cityPos.position = new Vector3(cityPos.position.x, cityPos.position.y / screenRate, cityPos.position.z);
        playerCity.transform.GetChild(1).position = cityPos.position;
        enemyCity.transform.GetChild(1).localPosition = playerCity.transform.GetChild(1).localPosition;
        lookingCity.GetChild(hss.CurrentPage).localScale = new Vector3(1.5f, 1.5f, 1);
        switchBtn.GetChild(0).gameObject.SetActive(false);
        playerRankBtn.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = "Dummy";
        dummyRankBtn.parent.GetChild(1).GetComponent<Text>().text = "Dummy";
        dummyRankBtn.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = AccountManager.Instance.userInfos.nickname;
        StartCoroutine("EnemyRepair");
        Debug.Log(hss.transform.GetChild(0).position);
        enemyHQ = ieg.ingameCityManager.enemyHQ;
        playerHQ = ieg.ingameCityManager.playerHQ;
    }

    private void Update() {
        if (isPlaying) {
            time -= Time.deltaTime;
            ingameTimer.text = ((int)(time / 60)).ToString() + ":";
            if (((int)(time % 60)) < 10)
                ingameTimer.text += "0";
            ingameTimer.text += ((int)(time % 60)).ToString();
            if (time < 0) {
                ingameTimer.text = "0:00";
                isPlaying = false;
                IngameScoreManager.Instance.AddScore(playerCity.GetComponent<IngameCityManager>().cityHP, IngameScoreManager.ScoreType.Health);
                resultManager.GameOverWindow(IngameResultManager.GameOverType.SURVIVE);
            }
            if (IngameScoreManager.Instance.playerScore > IngameScoreManager.Instance.dummyScore) {
                isPlaying = false;
                resultManager.GameOverWindow(IngameResultManager.GameOverType.WIN);
            }
            if (enemyHQ.hp < 1) {
                isPlaying = false;
                resultManager.GameOverWindow(IngameResultManager.GameOverType.WIN);
            }
            if(playerHQ.hp < 1) {
                isPlaying = false;
                resultManager.GameOverWindow(IngameResultManager.GameOverType.LOSE);
            }
            if(turn.text == "0") {
                if (enemyCity.transform.GetChild(1).childCount == 26) {
                    PlayerController pc = gameObject.GetComponent<PlayerController>();
                    bool end = true;
                    for (int i = 0; i < attackCard.childCount; i++) {
                        ActiveCardInfo ac = attackCard.GetChild(i).GetComponent<ActiveCardInfo>();
                        if (!string.IsNullOrEmpty(ac.data.unit.id) && pc.isEnoughResources(ac.data.unit.cost)) {
                            end = false;
                            break;
                        }
                        else if (ac.data.skill.id != 0 && pc.isEnoughResources(ac.data.skill.cost)) {
                            end = false;
                            break;
                        }
                    }
                    if (end) {
                        isPlaying = false;
                        resultManager.GameOverWindow(IngameResultManager.GameOverType.LOSE);
                    }
                }
            }
        }
        territoryCamera.transform.position = new Vector3(Screen.width - (hss.transform.GetChild(0).position.x), 0, 0);
        
        if (Input.GetKeyDown(KeyCode.Q)) 
            Debug.Log(hss.transform.GetChild(0).position);
    }

    public void SwitchCommand() {
        
        if (hss.CurrentPage != 0) {
            ShutProductButtons(true);
            playerRankBtn.GetChild(0).gameObject.SetActive(false);
        }
        else {
            ShutProductButtons(false);
            dummyRankBtn.GetChild(0).gameObject.SetActive(false);
        }
        
        StartCoroutine(HideButton());
    }

    private void ShutProductButtons(bool shut) {
        if(shut) {
            Debug.Log("shut");
            playerCity.GetComponent<IngameCityManager>().CurrentView = 1;
            Color shutColor = new Color(255, 255, 255, 0.3f);
            produceButonList.GetComponent<Image>().color = shutColor;
            for (int i = 0; i < 4; i++) {
                produceButonList.GetChild(i).GetComponent<Image>().color = shutColor;
                produceButonList.GetChild(i).GetChild(0).GetComponent<Text>().color = shutColor;
            }
            produceButonList.GetChild(4).gameObject.SetActive(true);
        }
        else {
            playerCity.GetComponent<IngameCityManager>().CurrentView = 0;
            Color onColor = new Color(255, 255, 255, 1.0f);
            produceButonList.GetComponent<Image>().color = onColor;
            for (int i = 0; i < 4; i++) {
                produceButonList.GetChild(i).GetComponent<Image>().color = onColor;
                produceButonList.GetChild(i).GetChild(0).GetComponent<Text>().color = onColor;
            }
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
            ExitIngameScene();
        });
    }

    public void ExitIngameScene() {
        GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
        gsm.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
        IngameScoreManager.Instance.DestroySelf();
    }

    IEnumerator EnemyRepair() {
        while(time > 60 && isPlaying == true) {
            yield return new WaitForSeconds(60f);
            repairAlert.SetActive(true);
            StartCoroutine("DisableAlert");
        }
    }

    IEnumerator DisableAlert() {
        yield return new WaitForSeconds(1.5f);
        repairAlert.SetActive(false);
    }
}