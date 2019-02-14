using DataModules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public partial class PlayerController : MonoBehaviour {

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

    [Header(" - UI")]
    [SerializeField] Transform commandButtons;
    [SerializeField] Transform playerResource;
    public PlayerResource resourceClass;

    [Header(" - ResourceText")]
    [SerializeField] Text goldValue;
    [SerializeField] Text foodValue;
    [SerializeField] Text turnValue;
    [SerializeField] Image envValue;
    [SerializeField] IngameCityManager icm;
    [SerializeField] GameObject hqUpgradeWnd;
    public IngameCityManager IngameCityManager {
        get { return icm; }
    }

    public int Food {
        get { return resourceClass.food; }
        set {
            resourceClass.food = value;
            IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.RESOURCE_CHANGE, this, resourceClass);
        }
    }
    public int Gold {
        get { return resourceClass.gold; }
        set {
            resourceClass.gold = value;
            IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.RESOURCE_CHANGE, this, resourceClass);
        }
    }
    public int Env {
        get { return resourceClass.environment; }
        set {
            resourceClass.environment = value;
            IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.RESOURCE_CHANGE, this, resourceClass);
        }
    }
    [Header (" - Player")]
    public int hqLevel;

    
    public ProductInfo pInfo { get; set; }    
    IngameScoreManager scoreManager;
    private bool warningOn = false;

    private void Awake() {
        scoreManager = IngameScoreManager.Instance;
        resourceClass = new PlayerResource();
        pInfo = new ProductInfo();
        pInfo.clickGold = new int[3];
        pInfo.clickFood = new int[3];
        pInfo.clickEnvironment = new int[3];
    }

    // Use this for initialization
    void Start() {
        PrintResource();
        hqLevel = 1;

        commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.GOLD));
        commandButtons.GetChild(1).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.FOOD));
        commandButtons.GetChild(2).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.ENVIRONMENT));
        //commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => gameTurn > 0).Subscribe(_ => ClickButton(Buttons.REPAIR));


        icm.productResources.gold.gold += icm.hq_tier_1.product.gold;
        icm.productResources.food.food += icm.hq_tier_1.product.food;
        icm.productResources.env.environment += icm.hq_tier_1.product.env;
        
    }

    private void OnMouseDown() {
        Debug.Log("클릭!");
    }

    private void ClickButton(Buttons btn) {
        int env = Env;
        switch (btn) {
            case Buttons.GOLD:
                if (env + icm.productResources.gold.environment > 0) {
                    if (icm.productResources.gold.gold > 0) {
                        Gold += icm.productResources.gold.gold;
                        Food += icm.productResources.gold.food;
                        Env += icm.productResources.gold.environment;
                        resourceClass.turn--;
                        if (Env < 100 && icm.unactiveBuildingIndex == 100)
                            icm.DecideUnActiveBuilding();
                    }
                }
                else {
                    Gold += icm.productResources.gold.gold;
                    Food += icm.productResources.gold.food;
                    Env = 300;
                    icm.SetUnactiveBuilding();
                    resourceClass.turn--;
                }
                scoreManager.AddScore(icm.productResources.gold.gold, IngameScoreManager.ScoreType.Product);
                break;
            case Buttons.FOOD:
                if (env + icm.productResources.food.environment > 0) {
                    if (icm.productResources.food.food > 0) {
                        Gold += icm.productResources.food.gold;
                        Food += icm.productResources.food.food;
                        Env += icm.productResources.food.environment;
                        resourceClass.turn--;
                        if (Env < 100 && icm.unactiveBuildingIndex == 100)
                            icm.DecideUnActiveBuilding();
                    }
                }
                else {
                    Gold += icm.productResources.food.gold;
                    Food += icm.productResources.food.food;
                    Env = 300;
                    icm.SetUnactiveBuilding();
                    resourceClass.turn--;
                }
                scoreManager.AddScore(icm.productResources.food.food, IngameScoreManager.ScoreType.Product);
                break;
            case Buttons.ENVIRONMENT:
                if (env < 300) {
                    if (icm.productResources.env.environment > 0) {
                        if (Gold + icm.productResources.env.gold >= 0 && Food + icm.productResources.env.food >= 0) {
                            Gold += icm.productResources.env.gold;
                            Food += icm.productResources.env.food;
                            Env += icm.productResources.env.environment;
                            if (Env > 300) {
                                scoreManager.AddScore(icm.productResources.env.environment - (Env - 300), IngameScoreManager.ScoreType.Product);
                                Env = 300;
                            }
                            else
                                scoreManager.AddScore(icm.productResources.env.environment, IngameScoreManager.ScoreType.Product);
                            if (Env >= 100 && icm.unactiveBuildingIndex != 100)
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
        goldValue.text = Gold.ToString();
        foodValue.text = Food.ToString();
        turnValue.text = resourceClass.turn.ToString();
        envValue.fillAmount = resourceClass.environment / 300.0f;

        Text envText = envValue.transform.parent.GetChild(2).GetComponent<Text>();
        envText.text = Mathf.RoundToInt(envValue.fillAmount * 100.0f).ToString() + "%" ;
    }

    public bool isEnoughResources(DataModules.Cost cost) {
        if (Gold < cost.gold) return false;
        if (Env < cost.environment) return false;
        if (Food < cost.food) return false;
        return true;
    }
}

/// <summary>
/// Upgrade 관련 처리
/// </summary>
public partial class PlayerController {
    private bool isUpgradeModalActivated = false;

    public void OpenHqUpgrageInfo(bool open) {
        if (hqLevel < 3)
            hqUpgradeWnd.SetActive(open);
        if (open) {
            hqUpgradeWnd.transform.GetChild(0).GetChild(1).GetComponent<Text>().text = "Lv." + hqLevel.ToString() + " 업그레이드";
            if (hqLevel == 1) {
                hqUpgradeWnd.transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<Text>().text = "+" + (icm.hq_tier_2.product.gold - icm.hq_tier_1.product.gold).ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(3).GetChild(1).GetComponent<Text>().text = "+" + icm.hq_tier_1.product.gold.ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<Text>().text = "+" + (icm.hq_tier_2.product.food - icm.hq_tier_1.product.food).ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(4).GetChild(1).GetComponent<Text>().text = "+" + icm.hq_tier_1.product.food.ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(5).GetChild(0).GetComponent<Text>().text = "+" + (icm.hq_tier_2.product.env - icm.hq_tier_1.product.env).ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(5).GetChild(1).GetComponent<Text>().text = "+" + icm.hq_tier_1.product.env.ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(6).GetChild(0).GetComponent<Text>().text = "+" + (icm.hq_tier_2.hp - icm.hq_tier_1.hp).ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(6).GetChild(1).GetComponent<Text>().text = "+" + icm.hq_tier_1.hp.ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(7).GetChild(0).GetChild(0).GetComponent<Text>().text = icm.hq_tier_2.upgradeCost.gold.ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(7).GetChild(1).GetChild(0).GetComponent<Text>().text = icm.hq_tier_2.upgradeCost.food.ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(7).GetChild(2).GetChild(0).GetComponent<Text>().text = icm.hq_tier_2.upgradeCost.env.ToString();
            }
            if (hqLevel == 2) {
                hqUpgradeWnd.transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<Text>().text = "+" + (icm.hq_tier_3.product.gold - icm.hq_tier_2.product.gold).ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(3).GetChild(1).GetComponent<Text>().text = "+" + icm.hq_tier_2.product.gold.ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<Text>().text = "+" + (icm.hq_tier_3.product.food - icm.hq_tier_2.product.food).ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(4).GetChild(1).GetComponent<Text>().text = "+" + icm.hq_tier_2.product.food.ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(5).GetChild(0).GetComponent<Text>().text = "+" + (icm.hq_tier_3.product.env - icm.hq_tier_2.product.env).ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(5).GetChild(1).GetComponent<Text>().text = "+" + icm.hq_tier_2.product.env.ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(6).GetChild(0).GetComponent<Text>().text = "+" + (icm.hq_tier_3.hp - icm.hq_tier_2.hp).ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(6).GetChild(1).GetComponent<Text>().text = "+" + icm.hq_tier_2.hp.ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(7).GetChild(0).GetChild(0).GetComponent<Text>().text = icm.hq_tier_3.upgradeCost.gold.ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(7).GetChild(1).GetChild(0).GetComponent<Text>().text = icm.hq_tier_3.upgradeCost.food.ToString();
                hqUpgradeWnd.transform.GetChild(0).GetChild(7).GetChild(2).GetChild(0).GetComponent<Text>().text = icm.hq_tier_3.upgradeCost.env.ToString();
            }
        }
    }

    public void HqUpgrade() {
        if (hqLevel == 1) {
            if (icm.hq_tier_2.upgradeCost.food < Food &&
                icm.hq_tier_2.upgradeCost.gold < Gold &&
                icm.hq_tier_2.upgradeCost.env < Env) {
                Debug.Log("2단계 업글");
                icm.productResources.gold.gold += icm.hq_tier_2.product.gold - icm.hq_tier_1.product.gold;
                icm.productResources.food.food += icm.hq_tier_2.product.food - icm.hq_tier_1.product.food;
                icm.productResources.env.environment += icm.hq_tier_2.product.env - icm.hq_tier_1.product.env;
                Food -= icm.hq_tier_2.upgradeCost.food;
                Gold -= icm.hq_tier_2.upgradeCost.gold;
                Env -= icm.hq_tier_2.upgradeCost.env;
                hqLevel++;
                resourceClass.turn--;
                IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, null);
                OpenHqUpgrageInfo(false);
                icm.DecideUnActiveBuilding();
            }
            else {
                if (!warningOn)
                    StartCoroutine(HqUpgradeWarning());
            }
        }
        else if (hqLevel == 2) {
            if (icm.hq_tier_3.upgradeCost.food < Food &&
                icm.hq_tier_3.upgradeCost.gold < Gold &&
                icm.hq_tier_3.upgradeCost.env < Env) {
                Debug.Log("3단계 업글");
                icm.productResources.gold.gold += icm.hq_tier_3.product.gold - icm.hq_tier_2.product.gold;
                icm.productResources.food.food += icm.hq_tier_3.product.food - icm.hq_tier_2.product.food;
                icm.productResources.env.environment += icm.hq_tier_3.product.env - icm.hq_tier_2.product.env;
                Food -= icm.hq_tier_3.upgradeCost.food;
                Gold -= icm.hq_tier_3.upgradeCost.gold;
                Env -= icm.hq_tier_3.upgradeCost.env;
                hqLevel++;
                resourceClass.turn--;
                commandButtons.parent.GetChild(2).GetComponent<Image>().enabled = false;
                commandButtons.parent.GetChild(2).GetChild(1).gameObject.SetActive(false);
                IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, null);
                hqUpgradeWnd.SetActive(false);
            }
            else {
                if (!warningOn)
                    StartCoroutine(HqUpgradeWarning());
            }
        }
        if (Env >= 100 && icm.unactiveBuildingIndex != 100)
            icm.CancleUnActiveBuilding();
        commandButtons.parent.GetChild(2).GetChild(2).GetComponent<Text>().text = hqLevel.ToString() + ".Lv";
        PrintResource();
    }

    IEnumerator HqUpgradeWarning() {
        warningOn = true;
        hqUpgradeWnd.transform.GetChild(0).GetChild(8).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        hqUpgradeWnd.transform.GetChild(0).GetChild(8).gameObject.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        hqUpgradeWnd.transform.GetChild(0).GetChild(8).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        hqUpgradeWnd.transform.GetChild(0).GetChild(8).gameObject.SetActive(false);
        warningOn = false;
    }

    private GameObject selectedObj;
    GameObject spritePanel;

    public void OnUpgradeModal() {
        spritePanel = icm.transform.GetChild(1).Find("Background/Dissolve").gameObject;
        GameObject uiPanel = transform.Find("UIDissolve").gameObject;

        spritePanel.SetActive(true);
        uiPanel.SetActive(true);

        //scroll 비활성화
        var horizontalScrollSnap = transform.Find("Horizontal Scroll Snap").GetComponent<UnityEngine.UI.Extensions.HorizontalScrollSnap>();
        horizontalScrollSnap.enabled = false;
        var scrollRect = transform.Find("Horizontal Scroll Snap").GetComponent<ScrollRect>();
        scrollRect.enabled = false;

        //upgrade 가능한 빌딩 order 변경
        isUpgradeModalActivated = true;
        IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.RESOURCE_CHANGE, this, resourceClass);
    }

    public void OnUpgradeBtnClicked() {
        if (Upgrade(selectedObj)) {
            CloseUpgradeModal();
        }
        else {
            //업그레이드 불가하다는 알림

        }
    }

    public void CloseUpgradeModal() {
        //modal 비활성화
        //scroll 활성화
        GetComponent<UpgradableBuildingGetter>().CloseModal();
        spritePanel.SetActive(false);
        isUpgradeModalActivated = false;
    }

    public bool isUpgradeModalActivate() {
        return isUpgradeModalActivated;
    }

    public bool Upgrade(GameObject obj) {
        if (obj == null) return false;
        IngameUpgradeCard ingameUpgradeCard = obj.GetComponent<IngameUpgradeCard>();
        if (ingameUpgradeCard == null) return false;

        int foodChange = ingameUpgradeCard.newProductPower.food;
        int envChange = ingameUpgradeCard.newProductPower.environment;
        int goldChange = ingameUpgradeCard.newProductPower.gold;

        icm.productResources.food.food += foodChange;
        icm.productResources.gold.food += foodChange;
        icm.productResources.env.food += foodChange;

        icm.productResources.food.environment += envChange;
        icm.productResources.gold.environment += envChange;
        icm.productResources.env.environment += envChange;

        icm.productResources.food.gold += goldChange;
        icm.productResources.gold.gold += goldChange;
        icm.productResources.env.gold += goldChange;

        BuildingObject bo = ingameUpgradeCard.targetBuilding.GetComponent<BuildingObject>();

        bo.data.card.product.food += foodChange;
        bo.data.card.product.gold += goldChange;
        bo.data.card.product.environment += envChange;

        bo.data.card.lv = ++ingameUpgradeCard.lv;
        if(bo.data.card.id == "primal_town_center") {
            hqLevel++;
        }
        IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.RESOURCE_CHANGE, this, resourceClass);
        return true;
    }
}