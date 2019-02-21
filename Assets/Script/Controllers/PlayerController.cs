using DataModules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

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
        public int turn = 250;
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
            PrintResource();
            IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.RESOURCE_CHANGE, this, resourceClass);
        }
    }
    public int Gold {
        get { return resourceClass.gold; }
        set {
            resourceClass.gold = value;
            PrintResource();
            IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.RESOURCE_CHANGE, this, resourceClass);
        }
    }
    public int Env {
        get { return resourceClass.environment; }
        set {
            resourceClass.environment = value;
            PrintResource();
            IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.RESOURCE_CHANGE, this, resourceClass);
        }
    }
    [Header (" - Player")]
    public int hqLevel = 1;
    [Header(" - Spine")]
    [SerializeField] private SkeletonDataAsset coinAni;
    [SerializeField] private Material coinAniMaterial;

    
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

        commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.GOLD));
        commandButtons.GetChild(1).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.FOOD));
        commandButtons.GetChild(2).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.ENVIRONMENT));
        //commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => gameTurn > 0).Subscribe(_ => ClickButton(Buttons.REPAIR));


        icm.productResources.gold.gold += icm.hq_tier_1.product.gold;
        icm.productResources.food.food += icm.hq_tier_1.product.food;
        icm.productResources.env.environment += icm.hq_tier_1.product.env;

        coinAni.GetSkeletonData(false);
        
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
#if HACK_PRODUCT_POWER
                        Gold += 1000;
#endif
                        resourceClass.turn--;
                        ShowCoinAnimation(0);
                        if (Env < 200 && icm.unactiveBuildingIndex1 == 100)
                            icm.DecideUnActiveBuilding();
                        if (Env < 100 && icm.unactiveBuildingIndex2 == 100)
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
#if HACK_PRODUCT_POWER
                        Food += 1000;
#endif
                        resourceClass.turn--;
                        ShowCoinAnimation(1);
                        if (Env < 200 && icm.unactiveBuildingIndex1 == 100)
                            icm.DecideUnActiveBuilding();
                        if (Env < 100 && icm.unactiveBuildingIndex2 == 100)
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
                            ShowCoinAnimation(2);
                            if (Env > 300) {
                                scoreManager.AddScore(icm.productResources.env.environment - (Env - 300), IngameScoreManager.ScoreType.Product);
                                Env = 300;
                            }
                            else
                                scoreManager.AddScore(icm.productResources.env.environment, IngameScoreManager.ScoreType.Product);
                            if (Env >= 200 && icm.unactiveBuildingIndex1 != 100)
                                icm.CancleUnActiveBuilding();
                            if (Env >= 100 && icm.unactiveBuildingIndex2 != 100)
                                icm.CancleUnActiveBuilding();
                            resourceClass.turn--;
                        }
                    }
                }
                break;
            case Buttons.REPAIR:
                ShowCoinAnimation(3);
                resourceClass.turn--;
                break;
        }
        PrintResource();
    }

    private void ShowCoinAnimation(int num) {
        SkeletonGraphic ani = SkeletonGraphic.NewSkeletonGraphicGameObject(coinAni, transform, coinAniMaterial);
        ani.GetComponent<RectTransform>().position = Input.mousePosition + new Vector3(0, 130f, 0f);
        ani.Initialize(false);
        ani.raycastTarget = false;
        ani.AnimationState.SetAnimation(0, coinAni.GetSkeletonData(false).Animations.Items[num], false);
        Destroy(ani.gameObject, 1f);
    }

    public void PrintResource() {
        goldValue.text = Gold.ToString();
        foodValue.text = Food.ToString();
        turnValue.text = resourceClass.turn.ToString();
        envValue.fillAmount = resourceClass.environment / 300.0f;

        Text envText = envValue.transform.parent.GetChild(2).GetComponent<Text>();
        envText.text = resourceClass.environment.ToString();
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
                hqLevel++;

                icm.productResources.gold.gold += icm.hq_tier_2.product.gold - icm.hq_tier_1.product.gold;
                icm.productResources.food.food += icm.hq_tier_2.product.food - icm.hq_tier_1.product.food;
                icm.productResources.env.environment += icm.hq_tier_2.product.env - icm.hq_tier_1.product.env;

                Food -= icm.hq_tier_2.upgradeCost.food;
                Gold -= icm.hq_tier_2.upgradeCost.gold;
                Env -= icm.hq_tier_2.upgradeCost.env;
                
                resourceClass.turn--;
                OpenHqUpgrageInfo(false);
                icm.DecideUnActiveBuilding();
                IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, null);
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
                hqLevel++;

                icm.productResources.gold.gold += icm.hq_tier_3.product.gold - icm.hq_tier_2.product.gold;
                icm.productResources.food.food += icm.hq_tier_3.product.food - icm.hq_tier_2.product.food;
                icm.productResources.env.environment += icm.hq_tier_3.product.env - icm.hq_tier_2.product.env;

                Food -= icm.hq_tier_3.upgradeCost.food;
                Gold -= icm.hq_tier_3.upgradeCost.gold;
                Env -= icm.hq_tier_3.upgradeCost.env;
                
                resourceClass.turn--;
                //commandButtons.parent.GetChild(2).GetComponent<Image>().enabled = false;
                //commandButtons.parent.GetChild(2).GetChild(1).gameObject.SetActive(false);
                IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, null);
                hqUpgradeWnd.SetActive(false);
            }
            else {
                if (!warningOn)
                    StartCoroutine(HqUpgradeWarning());
            }
        }
        if (Env >= 100 && icm.unactiveBuildingIndex2 != 100)
            icm.CancleUnActiveBuilding();
        if (Env >= 200 && icm.unactiveBuildingIndex1 != 100)
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

    public void CloseUpgradeModal() {
        //modal 비활성화
        //scroll 활성화
        GetComponent<UpgradableBuildingGetter>().CloseModal();
        spritePanel.SetActive(false);
        isUpgradeModalActivated = false;

        var horizontalScrollSnap = transform.Find("Horizontal Scroll Snap").GetComponent<UnityEngine.UI.Extensions.HorizontalScrollSnap>();
        horizontalScrollSnap.enabled = true;
        var scrollRect = transform.Find("Horizontal Scroll Snap").GetComponent<ScrollRect>();
        scrollRect.enabled = true;
    }

    public bool isUpgradeModalActivate() {
        return isUpgradeModalActivated;
    }

    public bool Upgrade(GameObject obj, Resource costs) {
        if (obj == null) return false;
        IngameUpgradeCard ingameUpgradeCard = obj.GetComponent<IngameUpgradeCard>();
        if (ingameUpgradeCard == null) return false;

        int foodChange = ingameUpgradeCard.newIncreasePower.food;
        int envChange = ingameUpgradeCard.newIncreasePower.environment;
        int goldChange = ingameUpgradeCard.newIncreasePower.gold;

        icm.productResources.food.food += foodChange;
        icm.productResources.gold.food += foodChange;
        icm.productResources.env.food += foodChange;

        icm.productResources.food.environment += envChange;
        icm.productResources.gold.environment += envChange;
        icm.productResources.env.environment += envChange;

        icm.productResources.food.gold += goldChange;
        icm.productResources.gold.gold += goldChange;
        icm.productResources.env.gold += goldChange;

        Food -= costs.food;
        Gold -= costs.gold;
        Env -= costs.environment;

        BuildingObject bo = ingameUpgradeCard.targetBuilding.GetComponent<BuildingObject>();

        bo.data.card.product.food += foodChange;
        bo.data.card.product.gold += goldChange;
        bo.data.card.product.environment += envChange;

        int lv = bo.data.card.lv;
        int rarity = bo.data.card.rarity;

        bo.data.card.hitPoint = ingameUpgradeCard.newHp;
        if (!string.IsNullOrEmpty(bo.data.card.unit.name)) {
            DataModules.Unit unit = bo.data.card.unit;
            unit.hitPoint = GetNewHp(unit.hitPoint, lv, rarity);
            unit.power = GetNewAttack(unit.hitPoint, lv, rarity);
            unit.lv += 1;

            IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.UNIT_UPGRADED, this, unit);
        }

        bo.data.card.lv = ++ingameUpgradeCard.lv;
        if(bo.spine != null) bo.GetComponent<TileSpineAnimation>().Upgrade();
        IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.RESOURCE_CHANGE, this, resourceClass);
        return true;
    }

    public int GetNewHp(int prevHp, int lv, int rarity) {
        int newHp = System.Convert.ToInt32(prevHp * (1 + lv / 16.0f) + (rarity / 16.0f));
        return newHp;
    }

    public int GetNewAttack(int prevHp, int lv, int rarity) {
        int newAmount = System.Convert.ToInt32(prevHp * (1 + (((lv + rarity) / 2.0f) / 12.0f)));
        return newAmount;
    }
}