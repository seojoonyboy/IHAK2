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

    private PlayerResource resourceClass;    
    public ProductInfo pInfo { get; set; }

    private void Awake() {
        resourceClass = new PlayerResource();
        pInfo = new ProductInfo();
        pInfo.clickGold = new int[3];
        pInfo.clickFood = new int[3];
        pInfo.clickEnvironment = new int[3];
    }

    // Use this for initialization
    void Start () {
        playerResource.Find("Food/Value").gameObject.GetComponent<Text>().text = resourceClass.food.ToString();
        playerResource.Find("Gold/Value").gameObject.GetComponent<Text>().text = resourceClass.gold.ToString();
        playerResource.Find("Turn/Value").gameObject.GetComponent<Text>().text = resourceClass.turn.ToString();
        playerResource.Find("Environment/Value").gameObject.GetComponent<Image>().fillAmount = resourceClass.environment / 300.0f;

        commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.FOOD));
        commandButtons.GetChild(1).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.GOLD));
        commandButtons.GetChild(2).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.ENVIRONMENT));
        //commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => gameTurn > 0).Subscribe(_ => ClickButton(Buttons.REPAIR));

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
        playerResource.Find("Food/Value").gameObject.GetComponent<Text>().text = resourceClass.food.ToString();
        playerResource.Find("Gold/Value").gameObject.GetComponent<Text>().text = resourceClass.gold.ToString();
        playerResource.Find("Turn/Value").gameObject.GetComponent<Text>().text = resourceClass.turn.ToString();
        playerResource.Find("Environment/Value").gameObject.GetComponent<Image>().fillAmount = resourceClass.environment / 300.0f;

    }
}
