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
    [SerializeField] IngameCityManager icm;

    private PlayerResource resourceClass;    
    public ProductInfo pInfo { get; set; }
    private int hqLevel;

    private void Awake() {
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

    private void ClickButton(Buttons btn) {
        int env = resourceClass.environment;
        switch (btn){
            case Buttons.GOLD: 
                if (env + pInfo.clickGold[2] >= 0) {
                    resourceClass.gold += pInfo.clickGold[0];
                    resourceClass.food += pInfo.clickGold[1];
                    resourceClass.environment += pInfo.clickGold[2];
                    resourceClass.turn--;
                }
                break;
            case Buttons.FOOD:
                if (env + pInfo.clickFood[2] >= 0) {
                    resourceClass.gold += pInfo.clickFood[0];
                    resourceClass.food += pInfo.clickFood[1];
                    resourceClass.environment += pInfo.clickFood[2];
                    resourceClass.turn--;
                }
                break;
            case Buttons.ENVIRONMENT:
                if (env < 300) {
                    if (resourceClass.gold + pInfo.clickEnvironment[0] >= 0 && resourceClass.food + pInfo.clickEnvironment[1] >= 0) {
                        resourceClass.gold += pInfo.clickEnvironment[0];
                        resourceClass.food += pInfo.clickEnvironment[1];
                        resourceClass.environment += pInfo.clickEnvironment[2];
                        if (resourceClass.environment > 300)
                            resourceClass.environment = 300;
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
        Debug.Log("업글");
        if(hqLevel == 0) {
            if(icm.hq_tier_2.upgradeCost.food < resourceClass.food && 
                icm.hq_tier_2.upgradeCost.gold < resourceClass.gold &&
                icm.hq_tier_2.upgradeCost.env < resourceClass.environment) {
                pInfo.clickGold[0] += icm.hq_tier_2.product.gold - icm.hq_tier_1.product.gold;
                pInfo.clickFood[1] += icm.hq_tier_2.product.food - icm.hq_tier_1.product.food;
                pInfo.clickEnvironment[2] += icm.hq_tier_2.product.env - icm.hq_tier_1.product.env;
                hqLevel++;
                resourceClass.turn--;
            }
        }
        else if(hqLevel == 1) {
            if (icm.hq_tier_3.upgradeCost.food < resourceClass.food &&
                icm.hq_tier_3.upgradeCost.gold < resourceClass.gold &&
                icm.hq_tier_3.upgradeCost.env < resourceClass.environment) {
                pInfo.clickGold[0] += icm.hq_tier_3.product.gold - icm.hq_tier_2.product.gold;
                pInfo.clickFood[1] += icm.hq_tier_3.product.food - icm.hq_tier_2.product.food;
                pInfo.clickEnvironment[2] += icm.hq_tier_3.product.env - icm.hq_tier_2.product.env;
                hqLevel++;
                resourceClass.turn--;
            }
        }
        PrintResource();
    }
}
