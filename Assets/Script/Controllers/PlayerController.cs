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

    private PlayerResource resourceClass;
    private int gameTurn;
    public ProductInfo pInfo { get; set; }

	// Use this for initialization
	void Start () {
        resourceClass = new PlayerResource();
        pInfo = new ProductInfo();
        playerResource.Find("Food/Value").gameObject.GetComponent<Text>().text = resourceClass.food.ToString();
        playerResource.Find("Gold/Value").gameObject.GetComponent<Text>().text = resourceClass.gold.ToString();
        playerResource.Find("Turn/Value").gameObject.GetComponent<Text>().text = resourceClass.turn.ToString();
        playerResource.Find("Environment/Value").gameObject.GetComponent<Image>().fillAmount = resourceClass.environment / 300;

        commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => gameTurn > 0).Subscribe(_ => ClickButton(Buttons.GOLD));
        commandButtons.GetChild(1).GetComponent<Button>().OnClickAsObservable().Where(_ => gameTurn > 0).Subscribe(_ => ClickButton(Buttons.FOOD));
        commandButtons.GetChild(2).GetComponent<Button>().OnClickAsObservable().Where(_ => gameTurn > 0).Subscribe(_ => ClickButton(Buttons.ENVIRONMENT));
        //commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => gameTurn > 0).Subscribe(_ => ClickButton(Buttons.REPAIR));

    }

    private void ClickButton(Buttons btn) {
        int env = 0;
        switch (btn){
            case Buttons.GOLD: 
                env = resourceClass.environment;
                if (env + pInfo.clickGold[2] >= 0) {
                    resourceClass.gold += pInfo.clickGold[0];
                    resourceClass.food += pInfo.clickGold[1];
                    resourceClass.environment += pInfo.clickGold[2];
                    gameTurn--;
                }
                break;
            case Buttons.FOOD:
                env = resourceClass.environment;
                if (env + pInfo.clickFood[2] >= 0) {
                    resourceClass.gold += pInfo.clickFood[0];
                    resourceClass.food += pInfo.clickFood[1];
                    resourceClass.environment += pInfo.clickFood[2];
                    gameTurn--;
                }
                break;
            case Buttons.ENVIRONMENT:
                
                if (env + pInfo.clickEnvironment[2] < 300) {
                    if (resourceClass.gold + pInfo.clickFood[0] >= 0 && resourceClass.food + pInfo.clickFood[1] >= 0) {
                        resourceClass.gold += pInfo.clickFood[0];
                        resourceClass.food += pInfo.clickFood[1];
                        resourceClass.environment += pInfo.clickFood[2];
                        if (resourceClass.environment > 300)
                            resourceClass.environment = 300;
                    }
                    gameTurn--;
                }
                break;
            case Buttons.REPAIR:
                gameTurn--;
                break;
        }
        playerResource.Find("Turn/Value").gameObject.GetComponent<Text>().text = gameTurn.ToString();
    }
}
