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

    public class PlayerResource {
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
    [SerializeField] Text goldValue;
    [SerializeField] Text foodValue;
    [SerializeField] Text turnValue;
    [SerializeField] Image envValue;
    [SerializeField] IngameCityManager icm;

    public PlayerResource resourceClass;
    public ProductInfo pInfo { get; set; }
    public int hqLevel;
    IngameScoreManager scoreManager;
    
    

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
        hqLevel = 1;

        commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.GOLD));
        commandButtons.GetChild(1).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.FOOD));
        commandButtons.GetChild(2).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.ENVIRONMENT));
        //commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => gameTurn > 0).Subscribe(_ => ClickButton(Buttons.REPAIR));

        commandButtons.parent.GetChild(0).GetComponent<Button>().OnClickAsObservable().Subscribe(_ => HqUpgrade()); // HqUpgrade버튼 접근후 함수 구독
        icm.productResources.gold.gold += 5;
        icm.productResources.food.food += 5;
        icm.productResources.env.environment += 5;
    }

    private void ClickButton(Buttons btn) {
        int env = resourceClass.environment;
        switch (btn){
            case Buttons.GOLD: 
                if (env + icm.productResources.gold.environment > 0) {
                    if (icm.productResources.gold.gold > 0) {
                        resourceClass.gold += icm.productResources.gold.gold;
                        resourceClass.food += icm.productResources.gold.food;
                        resourceClass.environment += icm.productResources.gold.environment;
                        resourceClass.turn--;
                        if (resourceClass.environment < 100 && icm.unactiveBuildingIndex == 100)
                            icm.DecideUnActiveBuilding();
                    }
                }
                else {
                    resourceClass.gold += icm.productResources.gold.gold;
                    resourceClass.food += icm.productResources.gold.food;
                    resourceClass.environment = 300;
                    icm.SetUnactiveBuilding();
                    resourceClass.turn--;
                }
                scoreManager.AddScore(icm.productResources.gold.gold, IngameScoreManager.ScoreType.Product);
                break;
            case Buttons.FOOD:
                if (env + icm.productResources.food.environment > 0) {
                    if (icm.productResources.food.food > 0) {
                        resourceClass.gold += icm.productResources.food.gold;
                        resourceClass.food += icm.productResources.food.food;
                        resourceClass.environment += icm.productResources.food.environment;
                        resourceClass.turn--;
                        if (resourceClass.environment < 100 && icm.unactiveBuildingIndex == 100)
                            icm.DecideUnActiveBuilding();
                    }
                }
                else {
                    resourceClass.gold += icm.productResources.food.gold;
                    resourceClass.food += icm.productResources.food.food;
                    resourceClass.environment = 300;
                    icm.SetUnactiveBuilding();
                    resourceClass.turn--;
                }
                scoreManager.AddScore(icm.productResources.food.food, IngameScoreManager.ScoreType.Product);
                break;
            case Buttons.ENVIRONMENT:
                if (env < 300) {
                    if (icm.productResources.env.environment > 0) {
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
                            if (resourceClass.environment >= 100 && icm.unactiveBuildingIndex != 100)
                                icm.CancleUnActiveBuilding();
                            resourceClass.turn--;
                        }
                    }
                }
                break;
            case Buttons.REPAIR:
                resourceClass.turn--;
                break;
        }
        PrintResource();
    }

    public void PrintResource() {
        goldValue.text = resourceClass.gold.ToString();
        foodValue.text = resourceClass.food.ToString();
        turnValue.text = resourceClass.turn.ToString();
        envValue.fillAmount = resourceClass.environment / 300.0f;
    }

    public bool isEnoughResources(DataModules.Cost cost) {
        if (resourceClass.gold < cost.gold) return false;
        if (resourceClass.environment < cost.environment) return false;
        if (resourceClass.food < cost.food) return false;
        return true;
    }

    private void HqUpgrade() {
        if(hqLevel == 1) {
            if(icm.hq_tier_2.upgradeCost.food < resourceClass.food && 
                icm.hq_tier_2.upgradeCost.gold < resourceClass.gold &&
                icm.hq_tier_2.upgradeCost.env < resourceClass.environment) {
                Debug.Log("2단계 업글");
                icm.productResources.gold.gold += icm.hq_tier_2.product.gold - icm.hq_tier_1.product.gold;
                icm.productResources.food.food += icm.hq_tier_2.product.food - icm.hq_tier_1.product.food;
                icm.productResources.env.environment += icm.hq_tier_2.product.env - icm.hq_tier_1.product.env;
                resourceClass.food -= icm.hq_tier_2.upgradeCost.food;
                resourceClass.gold -= icm.hq_tier_2.upgradeCost.gold;
                resourceClass.environment -= icm.hq_tier_2.upgradeCost.env;
                hqLevel++;
                resourceClass.turn--;

                IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, null);
            }
        }
        else if(hqLevel == 2) {
            if (icm.hq_tier_3.upgradeCost.food < resourceClass.food &&
                icm.hq_tier_3.upgradeCost.gold < resourceClass.gold &&
                icm.hq_tier_3.upgradeCost.env < resourceClass.environment) {
                Debug.Log("3단계 업글");
                icm.productResources.gold.gold += icm.hq_tier_3.product.gold - icm.hq_tier_2.product.gold;
                icm.productResources.food.food += icm.hq_tier_3.product.food - icm.hq_tier_2.product.food;
                icm.productResources.env.environment += icm.hq_tier_3.product.env - icm.hq_tier_2.product.env;
                resourceClass.food -= icm.hq_tier_3.upgradeCost.food;
                resourceClass.gold -= icm.hq_tier_3.upgradeCost.gold;
                resourceClass.environment -= icm.hq_tier_3.upgradeCost.env;
                hqLevel++;
                resourceClass.turn--;
                commandButtons.parent.GetChild(0).gameObject.SetActive(false);
                IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, null);
            }
        }
        commandButtons.parent.GetChild(0).GetChild(2).GetComponent<Text>().text = hqLevel.ToString() + ".Lv";
        PrintResource();
    }
}
