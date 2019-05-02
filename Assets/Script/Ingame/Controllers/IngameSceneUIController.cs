using Container;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class IngameSceneUIController : MonoBehaviour {

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.IngameScene;

    [SerializeField] Text playerName;
    [SerializeField] GameObject playerCity;
    [SerializeField] GameObject enemyCity;
    [SerializeField] Transform playerRankBtn;
    [SerializeField] Transform dummyRankBtn;
    [SerializeField] IngameResultManager resultManager;
    [SerializeField] public Transform attackCard;
    [SerializeField] Transform missionGoalUI;
    [SerializeField] GameObject missionGoalUI_prefab;

    public bool canPlaying = false;
    public bool canEnemyPlaying = false;

    private float time = 300;
    [SerializeField] public GameObject playerController;
    [SerializeField] public GameObject enemyController;

    IngameSceneEventHandler eventHandler;

    public static int deckId;

    void Awake() {
        eventHandler = IngameSceneEventHandler.Instance;
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, OnSubMissionComplete);
    }

    private void OnSubMissionComplete(Enum Event_Type, Component Sender, object Param) {
        string index = (string)Param;
        foreach(Transform goalUI in missionGoalUI) {
            if(index == goalUI.GetComponent<StringIndex>().Id) {
                goalUI.Find("CheckBox/Check").gameObject.SetActive(true);
            }
        }
    }

    void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, OnSubMissionComplete);
    }

    // Use this for initialization
    void Start() {
        playerName.text = AccountManager.Instance.userInfos.nickname;
        dummyRankBtn.parent.GetComponent<Text>().text = "Dummy";
        playerCity = GameObject.Find("PlayerCity");
        enemyCity = GameObject.Find("EnemyCity");
        int count = 1;

        int stageNum = AccountManager.Instance.mission.stageNum;
        switch (stageNum) {
            case 0:
            case 1:
                var values = PlayerController.Instance.gameObject.GetComponent<MissionManager>().stageGoals.missionLists
                    .Where(x => x.Key.StartsWith(stageNum.ToString()))
                    .Select(pv => pv.Value);
                
                foreach(string value in values) {
                    GameObject ui = Instantiate(missionGoalUI_prefab, missionGoalUI);
                    ui.transform.Find("Text").GetComponent<Text>().text = value;
                    ui.GetComponent<StringIndex>().Id = stageNum + "-" + count;
                    count++;
                }
                break;

            case 2:
                var secondValues = PlayerController.Instance.gameObject.GetComponent<MissionManager>().stageGoals.missionLists
                    .Where(x => x.Key.StartsWith(stageNum.ToString()))
                    .Select(pv => pv.Value);
                
                foreach (string value in secondValues) {
                    GameObject ui = Instantiate(missionGoalUI_prefab, missionGoalUI);
                    ui.transform.Find("Text").GetComponent<Text>().text = value;
                    ui.GetComponent<StringIndex>().Id = stageNum + "-" + count;
                    count++;
                }
                break;
        }
    }

    private void Update() {
        if (canPlaying && canEnemyPlaying) {
            //if (enemyController.GetComponent<PlayerResource>().TotalHp <= 0) {
            //    canPlaying = false;
            //    resultManager.GameOverWindow(IngameResultManager.GameOverType.WIN);
            //}
            //if (playerController.GetComponent<PlayerResource>().TotalHp <= 0) {
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

    public void NextMissionBtn() {
        int stage = AccountManager.Instance.mission.stageNum;
        AccountManager.Instance.StartMission(stage + 1);
    }

}