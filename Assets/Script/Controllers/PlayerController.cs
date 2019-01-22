using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    public enum Buttons {
        GOLD = 0,
        FOOD = 1,
        ENVIRONMENT = 2,
        REPAIR = 3,
    }

    class PlayerResource {
        public int gold = 50;
        public int food = 50;
        public int turn = 600;
        public int environment = 300;
    }

    public class ProductInfo { //gold food environment 순서의 생산량 저장
        public int[] clickGold;
        public int[] clickFood;
        public int[] clickEnvironment;
    }

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.IngameScene;


    [SerializeField] Transform commandButtons;
    [SerializeField] Transform playerResource;
    [SerializeField] Text foodValue;
    [SerializeField] Text goldValue;
    [SerializeField] Text turnValue;
    [SerializeField] Image envValue;
    [SerializeField] Text ingameTimer;
    [SerializeField] IngameCityManager icm;

    private PlayerResource resourceClass;    
    public ProductInfo pInfo { get; set; }
    private int hqLevel;
    IngameScoreManager scoreManager;
    private float time = 300;
    private bool isPlaying = true;

    private void Awake() {
        scoreManager = IngameScoreManager.Instance;
        resourceClass = new PlayerResource();
        pInfo = new ProductInfo();
        pInfo.clickGold = new int[3];
        pInfo.clickFood = new int[3];
        pInfo.clickEnvironment = new int[3];
    }

    // Use this for initialization
    void Start () {
        PrintResource();
        hqLevel = 0;

        commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.FOOD));
        commandButtons.GetChild(1).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.GOLD));
        commandButtons.GetChild(2).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.ENVIRONMENT));
        //commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => gameTurn > 0).Subscribe(_ => ClickButton(Buttons.REPAIR));

        commandButtons.parent.GetChild(0).GetComponent<Button>().OnClickAsObservable().Subscribe(_ => HqUpgrade()); // HqUpgrade버튼 접근후 함수 구독

    }

    private void Update() {
        if(isPlaying){
            time -= Time.deltaTime;
            ingameTimer.text = ((int)(time / 60)).ToString() + ":";
            if (((int)(time % 60)) < 10)
                ingameTimer.text += "0";
            ingameTimer.text += ((int)(time % 60)).ToString();
            if (time < 0) {
                ingameTimer.text = "0:00";
                isPlaying = false;
            }
        }
    }

    private void ClickButton(Buttons btn) {
        int env = resourceClass.environment;
        switch (btn){
            case Buttons.GOLD: 
                if (env + icm.productResources.gold.environment >= 0) {
                    resourceClass.gold += icm.productResources.gold.gold;
                    resourceClass.food += icm.productResources.gold.food;
                    resourceClass.environment += icm.productResources.gold.environment;
                    resourceClass.turn--;
                    scoreManager.AddScore(icm.productResources.gold.gold, IngameScoreManager.ScoreType.Product);
                }
                break;
            case Buttons.FOOD:
                if (env + icm.productResources.food.environment >= 0) {
                    resourceClass.gold += icm.productResources.food.gold;
                    resourceClass.food += icm.productResources.food.food;
                    resourceClass.environment += icm.productResources.food.environment;
                    resourceClass.turn--;
                    scoreManager.AddScore(icm.productResources.food.food, IngameScoreManager.ScoreType.Product);
                }
                break;
            case Buttons.ENVIRONMENT:
                if (env < 300) {
                    if (resourceClass.gold + icm.productResources.env.gold >= 0 && resourceClass.food + icm.productResources.env.food >= 0) {
                        resourceClass.gold += icm.productResources.env.gold;
                        resourceClass.food += icm.productResources.env.food;
                        resourceClass.environment += icm.productResources.env.environment;
                        if (resourceClass.environment > 300) {
                            scoreManager.AddScore(icm.productResources.env.environment - (resourceClass.environment - 300), IngameScoreManager.ScoreType.Product); 
                            resourceClass.environment = 300;
                        }
                        else
                            scoreManager.AddScore(icm.productResources.env.environment, IngameScoreManager.ScoreType.Product);
                        resourceClass.turn--;
                    }
                }
                break;
            case Buttons.REPAIR:
                resourceClass.turn--;
                break;
        }
        PrintResource();
    }

    private void PrintResource() {
        foodValue.text = resourceClass.food.ToString();
        goldValue.text = resourceClass.gold.ToString();
        turnValue.text = resourceClass.turn.ToString();
        envValue.fillAmount = resourceClass.environment / 300.0f;
    }

    private void HqUpgrade() {
        
        if(hqLevel == 0) {
            if(icm.hq_tier_2.upgradeCost.food < resourceClass.food && 
                icm.hq_tier_2.upgradeCost.gold < resourceClass.gold &&
                icm.hq_tier_2.upgradeCost.env < resourceClass.environment) {
                Debug.Log("2단계 업글");
                pInfo.clickGold[0] += icm.hq_tier_2.product.gold - icm.hq_tier_1.product.gold;
                pInfo.clickFood[1] += icm.hq_tier_2.product.food - icm.hq_tier_1.product.food;
                pInfo.clickEnvironment[2] += icm.hq_tier_2.product.env - icm.hq_tier_1.product.env;
                resourceClass.food -= icm.hq_tier_2.upgradeCost.food;
                resourceClass.gold -= icm.hq_tier_2.upgradeCost.gold;
                resourceClass.environment -= icm.hq_tier_2.upgradeCost.env;
                hqLevel++;
                resourceClass.turn--;
            }
        }
        else if(hqLevel == 1) {
            if (icm.hq_tier_3.upgradeCost.food < resourceClass.food &&
                icm.hq_tier_3.upgradeCost.gold < resourceClass.gold &&
                icm.hq_tier_3.upgradeCost.env < resourceClass.environment) {
                Debug.Log("3단계 업글");
                pInfo.clickGold[0] += icm.hq_tier_3.product.gold - icm.hq_tier_2.product.gold;
                pInfo.clickFood[1] += icm.hq_tier_3.product.food - icm.hq_tier_2.product.food;
                pInfo.clickEnvironment[2] += icm.hq_tier_3.product.env - icm.hq_tier_2.product.env;
                resourceClass.food -= icm.hq_tier_3.upgradeCost.food;
                resourceClass.gold -= icm.hq_tier_3.upgradeCost.gold;
                resourceClass.environment -= icm.hq_tier_3.upgradeCost.env;
                hqLevel++;
                resourceClass.turn--;
            }
        }
        PrintResource();
    }

    private void OnDestroy() {
        
    }
}
