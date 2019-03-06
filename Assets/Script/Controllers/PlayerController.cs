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
        public int turn = 500;
        public int environment = 0;
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
    [SerializeField] Transform productResource;
    public PlayerResource resourceClass;

    [Header(" - ResourceText")]
    [SerializeField] Text goldValue;
    [SerializeField] Text foodValue;
    [SerializeField] Text turnValue;
    [SerializeField] Image envValue;
    [SerializeField] IngameCityManager icm;
    [SerializeField] GameObject hqUpgradeWnd;

    int point;
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

    public int Point {
        get { return point; }
        set {
            point -= value;
            Debug.Log("포인트 소모");
        }
    }
    [Header(" - Player")]
    public int hqLevel = 1;
    public int tileCount;
    private int MaxHpMulti;
    public int goldConsume;
    [Header(" - Spine")]
    [SerializeField] private SkeletonDataAsset coinAni;
    [SerializeField] private Material coinAniMaterial;


    public ProductInfo pInfo { get; set; }
    IngameScoreManager scoreManager;
    private bool warningOn = false;
    private float envBonusProduce;

    private bool envEfctOn = false;
    private IEnumerator efct3;
    private IEnumerator efct5;

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
        PrimalEnvEfct();

        commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.GOLD));
        commandButtons.GetChild(1).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.FOOD));
        commandButtons.GetChild(2).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.ENVIRONMENT));
        commandButtons.GetChild(3).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.REPAIR));


        icm.productResources.gold.gold += icm.upgradeInfos[0].product.gold;
        icm.productResources.food.food += icm.upgradeInfos[0].product.food;
        icm.productResources.env.environment += icm.upgradeInfos[0].product.env;

        coinAni.GetSkeletonData(false);
        SetPlayerConsumeResource();
    }

    private void OnMouseDown() {
        Debug.Log("클릭!");
    }

    private void ClickButton(Buttons btn) {
        switch (btn) {
            case Buttons.GOLD:
                int goldEnv = (int)Mathf.Round((float)icm.productResources.gold.environment * envBonusProduce);
                if (Env + goldEnv >= -1250 && Env + goldEnv <= 1250) {
                    if (icm.productResources.gold.gold > 0) {
                        Gold += (int)Mathf.Round((float)icm.productResources.gold.gold * envBonusProduce);
                        Food += (int)Mathf.Round((float)icm.productResources.gold.food * envBonusProduce);
                        if (Env >= -1250 && Env <= 1250)
                            Env += goldEnv;
#if HACK_PRODUCT_POWER
                        Gold += 1000;
#endif
                        resourceClass.turn--;
                        ShowCoinAnimation(0);
                        PrintProduct(1);
                        //if (Env < 200 && icm.unactiveBuildingIndex1 == 100)
                        //    icm.DecideUnActiveBuilding();
                        //if (Env < 100 && icm.unactiveBuildingIndex2 == 100)
                        //    icm.DecideUnActiveBuilding();
                        scoreManager.AddScore(icm.productResources.gold.gold, IngameScoreManager.ScoreType.Product);
                    }
                }
                //else {
                //    Gold += icm.productResources.gold.gold;
                //    Food += icm.productResources.gold.food;
                //    //Env = 300;
                //    //icm.SetUnactiveBuilding();
                //    resourceClass.turn--;
                //}
                //scoreManager.AddScore(icm.productResources.gold.gold, IngameScoreManager.ScoreType.Product);
                break;
            case Buttons.FOOD:
                int foodEnv = (int)Mathf.Round((float)icm.productResources.food.environment * envBonusProduce);
                if (Env + foodEnv >= -1250 && Env + foodEnv <= 1250) {
                    if (icm.productResources.food.food > 0) {
                        Gold += (int)Mathf.Round((float)icm.productResources.food.gold * envBonusProduce);
                        Food += (int)Mathf.Round((float)icm.productResources.food.food * envBonusProduce);
                        if (Env >= -1250 && Env <= 1250)
                            Env += foodEnv;
#if HACK_PRODUCT_POWER
                        Food += 1000;
#endif
                        resourceClass.turn--;
                        ShowCoinAnimation(1);
                        PrintProduct(2);
                        //if (Env < 200 && icm.unactiveBuildingIndex1 == 100)
                        //    icm.DecideUnActiveBuilding();
                        //if (Env < 100 && icm.unactiveBuildingIndex2 == 100)
                        //    icm.DecideUnActiveBuilding();
                        scoreManager.AddScore(icm.productResources.food.food, IngameScoreManager.ScoreType.Product);
                    }
                }
                //else {
                //    Gold += icm.productResources.food.gold;
                //    Food += icm.productResources.food.food;
                //    //Env = 300;
                //    //icm.SetUnactiveBuilding();
                //    resourceClass.turn--;
                //}
                //scoreManager.AddScore(icm.productResources.food.food, IngameScoreManager.ScoreType.Product);
                break;
            case Buttons.ENVIRONMENT:
                int intEnv = (int)Mathf.Round((float)icm.productResources.env.environment * envBonusProduce);
                if (Env >= -1250 && Env <= 1250) {
                    if (icm.productResources.env.environment > 0) {
                        if (Gold + icm.productResources.env.gold >= 0 && Food + icm.productResources.env.food >= 0) {
                            Gold += (int)Mathf.Round((float)icm.productResources.env.gold * envBonusProduce);
                            Food += (int)Mathf.Round((float)icm.productResources.env.food * envBonusProduce);
                            Env += intEnv;
                            ShowCoinAnimation(2);
                            if (Env > 1250) {
                                scoreManager.AddScore(intEnv - (Env - 1250), IngameScoreManager.ScoreType.Product);
                                Env = 1250;
                            }
                            else
                                scoreManager.AddScore(intEnv, IngameScoreManager.ScoreType.Product);
                            //if (Env >= 200 && icm.unactiveBuildingIndex1 != 100)
                            //    icm.CancleUnActiveBuilding();
                            //if (Env >= 100 && icm.unactiveBuildingIndex2 != 100)
                            //    icm.CancleUnActiveBuilding();
                            resourceClass.turn--;
                            PrintProduct(3);
                        }
                    }
                }
                break;
            case Buttons.REPAIR:
                float destroyCount = icm.CityDestroyBuildingCount();
                float calculate = MaxHpMulti * ((1f + (0.02f * destroyCount)) * (tileCount + hqLevel) / (tileCount * 1.5f));
                goldConsume = Mathf.RoundToInt(calculate);

                if (Gold >= 0 + goldConsume) {
                    ShowCoinAnimation(3);
                    icm.RepairPlayerCity();
                    Gold -= goldConsume;
                    resourceClass.turn--;
                }
                break;
        }
        PrimalEnvEfct();
        PrintResource();
        //if (resourceClass.turn == 0) {
        //    IngameSceneUIController isc = gameObject.GetComponent<IngameSceneUIController>();
        //    isc.isPlaying = false;

        //}
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
        //envValue.fillAmount = resourceClass.environment / 300.0f;

        Transform envBar = envValue.transform.parent.GetChild(1);
        envBar.localPosition = new Vector3(((float)Env / 1250.0f) * 540, 0, 0);
        Text envText = envValue.transform.parent.GetChild(3).GetComponent<Text>();
        envText.text = Env.ToString();
    }

    public void PrintProduct(int num) {
        switch (num) {
            case 1:
                productResource.GetChild(0).GetComponent<Text>().text = "금생산량";
                productResource.GetChild(1).GetComponent<Text>().text = icm.productResources.gold.gold + " + " + (int)Mathf.Round((float)icm.productResources.gold.gold * (envBonusProduce - 1.0f));
                productResource.GetChild(2).GetComponent<Text>().text = icm.productResources.gold.food+ " + " + (int)Mathf.Round((float)icm.productResources.gold.food * (envBonusProduce - 1.0f));
                productResource.GetChild(3).GetComponent<Text>().text = icm.productResources.gold.environment + " + " + (int)Mathf.Round((float)icm.productResources.gold.environment * (envBonusProduce - 1.0f));
                break;
            case 2:
                productResource.GetChild(0).GetComponent<Text>().text = "음식생산량";
                productResource.GetChild(1).GetComponent<Text>().text = icm.productResources.food.gold + " + " + (int)Mathf.Round((float)icm.productResources.food.gold * (envBonusProduce - 1.0f));
                productResource.GetChild(2).GetComponent<Text>().text = icm.productResources.food.food + " + " + (int)Mathf.Round((float)icm.productResources.food.food * (envBonusProduce - 1.0f));
                productResource.GetChild(3).GetComponent<Text>().text = icm.productResources.food.environment + " + " + (int)Mathf.Round((float)icm.productResources.food.environment * (envBonusProduce - 1.0f));
                break;
            case 3:
                productResource.GetChild(0).GetComponent<Text>().text = "환경생산량";
                productResource.GetChild(1).GetComponent<Text>().text = icm.productResources.env.gold + " + " + (int)Mathf.Round((float)icm.productResources.env.gold * (envBonusProduce - 1.0f));
                productResource.GetChild(2).GetComponent<Text>().text = icm.productResources.env.food + " + " + (int)Mathf.Round((float)icm.productResources.env.food * (envBonusProduce - 1.0f));
                productResource.GetChild(3).GetComponent<Text>().text = icm.productResources.env.environment + " + " + (int)Mathf.Round((float)icm.productResources.env.environment * (envBonusProduce - 1.0f));
                break;
        }
    }

    public bool isEnoughResources(DataModules.Cost cost) {
        if (Gold < cost.gold) return false;
        //if (Env < cost.environment) return false;
        if (Food < cost.food) return false;
        return true;
    }

    private void PrimalEnvEfct() {
        if (Env <= -1100) {
            if (!envEfctOn) {
                envEfctOn = !envEfctOn;
                efct3 = Efct3Second(false);
                efct5 = Efct5Second(false);
                StartCoroutine(efct3);
                StartCoroutine(efct5);
            }
        }
        else if (Env > -1100) {
            if (envEfctOn) {
                envEfctOn = !envEfctOn;
                StopCoroutine(efct3);
                StopCoroutine(efct5);
            }
        }
        if (Env < 400)
            envBonusProduce = 1.0f;
        if (Env >= 400)
            envBonusProduce = 1.1f;
        if (Env >= 700 && Env < 1100) {
            envBonusProduce = 1.25f;
            if (envEfctOn) {
                envEfctOn = !envEfctOn;
                StopCoroutine(efct3);
                StopCoroutine(efct5);
            }
        }
        else if (Env >= 1100) {
            if (!envEfctOn) {
                envEfctOn = !envEfctOn;
                efct3 = Efct3Second(true);
                efct5 = Efct5Second(true);
                StartCoroutine(efct3);
                StartCoroutine(efct5);
            }
        }
    }

    private IEnumerator Efct3Second(bool positive) {
        while (!positive) {
            yield return new WaitForSeconds(3.0f);
            icm.DamagePlayerCity(15 / 2);
        }
        while (positive) {
            yield return new WaitForSeconds(3.0f);
            icm.DamagePlayerCity((int)Mathf.Round((15 / 2)));
        }
    }

    private IEnumerator Efct5Second(bool positive) {
        while (!positive) {
            yield return new WaitForSeconds(5.0f);
            Gold -= 100;
            if (Gold < 0)
                Gold = 0;
            Food -= 100;
            if (Food < 0)
                Food = 0;
            resourceClass.turn--;
        }
        while (positive) {
            yield return new WaitForSeconds(5.0f);
            Gold -= 50;
            if (Gold < 0)
                Gold = 0;
            Food -= 50;
            if (Food < 0)
                Food = 0;
            resourceClass.turn--;
        }
    }
}

/// <summary>
/// Upgrade 관련 처리
/// </summary>
public partial class PlayerController {
    private bool isUpgradeModalActivated = false;

    [Header(" - UpgradeModal")]
    [SerializeField] Transform innerModal;
    [SerializeField] Text point_val;
    [SerializeField] Text cost_gold_val;
    [SerializeField] Text cost_food_val;
    [Tooltip("분야별 배율 관련 영역")]
    [SerializeField] IngameUpgradeHandler[] magnifications;

    public void HqUpgrade() {
        int hq_lv_index = hqLevel - 1;

        if (icm.upgradeInfos[hq_lv_index].upgradeCost.food < Food &&
                icm.upgradeInfos[hq_lv_index].upgradeCost.gold < Gold) {
            Debug.Log("2단계 업글");
            hqLevel++;

            if(hq_lv_index == 0) {
                icm.productResources.gold.gold += icm.upgradeInfos[hq_lv_index].product.gold;
                icm.productResources.food.food += icm.upgradeInfos[hq_lv_index].product.food;
            }
            else {
                icm.productResources.gold.gold += icm.upgradeInfos[hq_lv_index].product.gold - icm.upgradeInfos[hq_lv_index - 1].product.gold;
                icm.productResources.food.food += icm.upgradeInfos[hq_lv_index].product.food - icm.upgradeInfos[hq_lv_index - 1].product.food;
            }

            Food -= icm.upgradeInfos[hq_lv_index].upgradeCost.food;
            Gold -= icm.upgradeInfos[hq_lv_index].upgradeCost.gold;

            icm.DecideUnActiveBuilding();
            IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, null);
        }
        else {
            if (!warningOn)
                StartCoroutine(HqUpgradeWarning());
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

    public void OnUpgradeModal() {
        GameObject uiPanel = transform.Find("UpgradeModal").gameObject;
        uiPanel.SetActive(true);

        //scroll 비활성화
        var horizontalScrollSnap = transform.Find("Horizontal Scroll Snap").GetComponent<UnityEngine.UI.Extensions.HorizontalScrollSnap>();
        horizontalScrollSnap.enabled = false;
        var scrollRect = transform.Find("Horizontal Scroll Snap").GetComponent<ScrollRect>();
        scrollRect.enabled = false;

        //upgrade 가능한 빌딩 order 변경
        isUpgradeModalActivated = true;
        point_val.text = Point.ToString();
        
        foreach(IngameUpgradeHandler handler in magnifications) {
            handler.Init(icm.myBuildings_mags);
        }

        IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.RESOURCE_CHANGE, this, resourceClass);
    }

    public void CloseUpgradeModal() {
        //modal 비활성화
        //scroll 활성화
        isUpgradeModalActivated = false;

        var horizontalScrollSnap = transform.Find("Horizontal Scroll Snap").GetComponent<UnityEngine.UI.Extensions.HorizontalScrollSnap>();
        horizontalScrollSnap.enabled = true;
        var scrollRect = transform.Find("Horizontal Scroll Snap").GetComponent<ScrollRect>();
        scrollRect.enabled = true;
    }

    public bool isUpgradeModalActivate() {
        return isUpgradeModalActivated;
    }

    public bool Upgrade() {
        if(Point <= 0) {
            Debug.Log("포인트가 없습니다.");
            return false;
        }
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

    public void SetPlayerConsumeResource() {
        float hp = icm.cityMaxHP;
        MaxHpMulti = Mathf.RoundToInt(hp * 0.005f);
        tileCount = icm.CityTotalTileCount();
    }

}