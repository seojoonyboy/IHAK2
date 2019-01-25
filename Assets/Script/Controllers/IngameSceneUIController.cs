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
    [SerializeField] Transform cityPos;
    [SerializeField] Transform lookingCity;
    [SerializeField] Transform switchBtn;
    [SerializeField] Transform playerRankBtn;
    [SerializeField] Transform dummyRankBtn;
    [SerializeField] Text ingameTimer;
    [SerializeField] IngameResultManager resultManager;


    private HorizontalScrollSnap hss;
    private GameObject city;
    private IngameEnemyGenerator ieg;
    [HideInInspector]
    public bool isPlaying = true;
    private float time = 300;
    private float baseCameraSize = 957.0f;
    private float baseScreenHeight = 1920.0f;
    private float screenRate;

    public static int deckId;

    private void Awake() {
        GameObject go = AccountManager.Instance.transform.GetChild(0).GetChild(AccountManager.Instance.leaderIndex).gameObject;
        screenRate = baseScreenHeight / (Screen.height);
        Camera.main.orthographicSize = baseCameraSize / screenRate;
        go.SetActive(true);
        GameObject ld = (GameObject)Instantiate(go, playerCity.transform);

        ProductResources touchProdPower = ld.GetComponent<TileGroup>().touchPerProdPower;
        //bool isDataExist = isDataInited(touchProdPower);
        //if (!isDataExist) {

        //}
        NetworkManager networkManager = NetworkManager.Instance;
        StringBuilder url = new StringBuilder();
        url.Append(networkManager.baseUrl)
            .Append("api/users/deviceid/")
            .Append(AccountManager.Instance.DEVICEID)
            .Append("/decks/")
            .Append(deckId);
        NetworkManager.Instance.request("GET", url.ToString(), OnDataCallback, false);

        ld.transform.localScale = new Vector3(1, 1, 1);
        playerCity.GetComponent<IngameCityManager>().eachPlayersTileGroups.Add(ld);
        go.SetActive(false);
    }

    private void OnDataCallback(HttpResponse response) {
        if (response.responseCode == 200) {
            if(response.data != null) {
                DeckDetail deckDetail = JsonConvert.DeserializeObject<DeckDetail>(response.data.ToString());

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
        playerName.text = AccountManager.Instance.userInfos.nickname;
        hss = transform.GetChild(0).GetComponent<HorizontalScrollSnap>();
        cityPos.position = new Vector3(cityPos.position.x, cityPos.position.y / screenRate, cityPos.position.z);
        playerCity.transform.GetChild(1).position = cityPos.position;
        ieg = FindObjectOfType<IngameEnemyGenerator>();
        ieg.tileGroup.transform.localPosition = playerCity.transform.GetChild(1).localPosition;
        ieg.tileGroup.transform.localScale = new Vector3(1, 1, 1);
        lookingCity.GetChild(hss.CurrentPage).localScale = new Vector3(1.5f, 1.5f, 1);
        switchBtn.GetChild(0).gameObject.SetActive(false);
        playerRankBtn.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = "Dummy";
        dummyRankBtn.parent.GetChild(1).GetComponent<Text>().text = "Dummy";
        dummyRankBtn.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = AccountManager.Instance.userInfos.nickname;
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
            if (ieg.ingameCityManager.enemyBuildingsInfo[12].hp < 1) {
                isPlaying = false;
                resultManager.GameOverWindow(IngameResultManager.GameOverType.WIN);
            }
        }
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

    
}