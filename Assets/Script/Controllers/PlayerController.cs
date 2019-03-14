using DataModules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using System;

public partial class PlayerController : MonoBehaviour {

    public enum Buttons {
        GOLD = 0,
        FOOD = 1,
        ENVIRONMENT = 2,
        REPAIR = 3,
        MILITARY = 4
    }

    public class PlayerResource {
        public uint gold = 50;
        public uint food = 50;
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
    [SerializeField] Text ingameTimer;
    public PlayerResource resourceClass;

    [Header(" - ResourceText")]
    [SerializeField] Text goldValue;
    [SerializeField] Text foodValue;
    [SerializeField] Text turnValue;
    [SerializeField] Image envValue;
    [SerializeField] IngameCityManager icm;
    [SerializeField] GameObject hqUpgradeWnd;
    
    int point;
    private bool playing = false;
    private int tech_lv;

    public IngameCityManager IngameCityManager {
        get { return icm; }
    }

    public uint Food {
        get { return resourceClass.food; }
        set {
            resourceClass.food = value;
            PrintResource();
            IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.RESOURCE_CHANGE, this, resourceClass);
        }
    }
    public uint Gold {
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
    public int TechLv {
        get { return tech_lv; }
        set {
            tech_lv = value;
        }
    }
    [Header(" - Player")]
    public int hqLevel = 1;
    public int tileCount;
    private int MaxHpMulti;
    public int goldConsume;
    public bool activeRepair = false;
    public float repairTimer;
    
    [Header(" - Spine")]
    [SerializeField] private SkeletonDataAsset coinAni;
    [SerializeField] private SkeletonDataAsset upgrageAni;
    [SerializeField] private Material coinAniMaterial;
    [SerializeField] private Material upgradeAniMaterial;

    [Header(" - SpineChange Standards")]
    public List<int> standards;
    public List<int> HQ_standards;

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
        commandButtons.GetChild(4).GetComponent<Button>().OnClickAsObservable().Where(_ => resourceClass.turn > 0).Subscribe(_ => ClickButton(Buttons.MILITARY));

        icm.productResources.gold.gold += icm.upgradeInfos[0].product.gold;
        icm.productResources.food.food += icm.upgradeInfos[0].product.food;
        icm.productResources.env.environment += icm.upgradeInfos[0].product.env;

        coinAni.GetSkeletonData(false);
        SetPlayerConsumeResource();
        playing = true;
        StartCoroutine(AoutomaticSystem());
        Observable.EveryUpdate().Where(_ => activeRepair == true).Subscribe(_ => repairTimer += Time.deltaTime);
        Observable.EveryUpdate().Where(_ => activeRepair == true).Where(_=>repairTimer >= 2).Subscribe(_ => icm.RepairPlayerCity());
        Observable.EveryUpdate().Where(_ => activeRepair == true && icm.cityHP == icm.cityMaxHP).Subscribe(_ => icm.ResetProductPower());
        Observable.EveryUpdate().Where(_ => activeRepair == true && icm.cityHP == icm.cityMaxHP).Subscribe(_ => activeRepair = false);
        Observable.EveryUpdate().Where(_ => repairTimer >= 2).Subscribe(_ => repairTimer = 0);
        Observable.EveryUpdate().Where(_ => activeRepair == false).Subscribe(_ => repairTimer = 0);


        //Food = 10000;
        //Gold = 10000;

        UpdateUpgradeCost();
    }
    
    private void OnMouseDown() {
        Debug.Log("클릭!"); 
    }

    private void ClickButton(Buttons btn) { //생산 업그레이드
        bool result = false;
        switch (btn) {
            case Buttons.GOLD:
                result = CanUpgrade(icm.myBuildings_mags[0]);
                ShowUpgradeAnimation(0);
                break;
            case Buttons.FOOD:
                result = CanUpgrade(icm.myBuildings_mags[1]);
                ShowUpgradeAnimation(1);
                break;
            case Buttons.ENVIRONMENT:
                result = CanUpgrade(icm.myBuildings_mags[2]);
                ShowUpgradeAnimation(2);
                break;
            case Buttons.MILITARY:
                result = CanUpgrade(icm.myBuildings_mags[3]);
                ShowUpgradeAnimation(4);
                break;
            case Buttons.REPAIR:
                /*
                float destroyCount = icm.CityDestroyBuildingCount();
                float calculate = MaxHpMulti * ((1f + (0.02f * destroyCount)) * (tileCount + hqLevel) / (tileCount * 1.5f));
                goldConsume = Mathf.RoundToInt(calculate);

                if (Gold >= 0 + goldConsume) {
                    ShowCoinAnimation(3);
                    icm.RepairPlayerCity();
                    Gold -= goldConsume;
                    resourceClass.turn--;
                }
                */
                if(activeRepair == false) {
                    if (icm.cityHP == icm.cityMaxHP) {
                        activeRepair = false;
                        icm.ResetProductPower();
                    }
                    else {
                        activeRepair = true;
                        icm.goldGenerate = icm.productResources.all.gold;
                        icm.envGenerate = icm.productResources.all.environment;
                        icm.foodGenerate = icm.productResources.all.food;
                        icm.productResources.all.gold = Mathf.RoundToInt(icm.productResources.all.gold * 0.2f);
                        icm.productResources.all.environment = Mathf.RoundToInt(icm.productResources.all.environment * 0.2f);
                        icm.productResources.all.food = Mathf.RoundToInt(icm.productResources.all.food * 0.2f);
                        ShowCoinAnimation(3);
                    }
                }
                else if(activeRepair == true) {
                    activeRepair = false;
                    icm.ResetProductPower();
                }
                break;
        }
        if (!result) {
            //IngameAlarm.instance.SetAlarm("자원이 부족합니다!");
            return;
        }
        Upgrade(btn);

        PrintResource();
        PrimalEnvEfct();

        UpdateBuildingImages();
    }

    private void ShowCoinAnimation(int num) {
        SkeletonGraphic ani = SkeletonGraphic.NewSkeletonGraphicGameObject(coinAni, transform, coinAniMaterial);
        ani.GetComponent<RectTransform>().position = Input.mousePosition + new Vector3(0, 130f, 0f);
        ani.Initialize(false);
        ani.raycastTarget = false;
        ani.AnimationState.SetAnimation(0, coinAni.GetSkeletonData(false).Animations.Items[num], false);
        Destroy(ani.gameObject, 1f);
    }

    private void ShowUpgradeAnimation(int num) {
        SkeletonGraphic ani = SkeletonGraphic.NewSkeletonGraphicGameObject(upgrageAni, transform, coinAniMaterial);
        ani.GetComponent<RectTransform>().position = commandButtons.GetChild(num).GetChild(2).position;
        ani.Initialize(false);
        ani.raycastTarget = false;
        ani.AnimationState.SetAnimation(1, "1.Level Up", false);
        Destroy(ani.gameObject, 1f);
    }

    private void UpdateBuildingImages() {
        var buildings = icm.myBuildingsInfo;
        if (HQ_standards.Count != 0 && HQ_standards[0] == TechLv) {
            foreach(IngameCityManager.BuildingInfo building in buildings) {
                if (building.cardInfo.type == "HQ") building.gameObject.GetComponent<TileSpineAnimation>().Upgrade();
            }
            HqUpgrade();
            HQ_standards.RemoveAt(0);
        }

        if(standards.Count != 0 && standards[0] == TechLv) {
            foreach (IngameCityManager.BuildingInfo building in buildings) {
                if (building.cardInfo.type != "HQ") building.gameObject.GetComponent<TileSpineAnimation>().Upgrade();
            }
            standards.RemoveAt(0);
        }
    }

    public void PrintResource() {
        goldValue.text = Gold.ToString();
        foodValue.text = Food.ToString();
        turnValue.text = resourceClass.turn.ToString();
        //envValue.fillAmount = resourceClass.environment / 300.0f;

        Transform envBar = envValue.transform.parent.GetChild(1);
        envBar.localPosition = new Vector3(((float)Env / 600.0f) * 540, 0, 0);
        Text envText = envValue.transform.parent.GetChild(3).GetComponent<Text>();
        envText.text = Env.ToString();
        showResource();
    }

    //public void PrintProduct(int num) {
    //    switch (num) {
    //        case 1:
    //            showResource(icm.productResources.gold, icm.myBuildings_mags[2].current_mag);
    //            break;
    //        case 2:
    //            showResource(icm.productResources.food, icm.myBuildings_mags[1].current_mag);
    //            break;
    //        case 3:
    //            showResource(icm.productResources.env, icm.myBuildings_mags[3].current_mag);
    //            break;
    //    }
    //}

    private void showResource() {
        productResource.GetChild(0).GetComponent<Text>().text = Mathf.RoundToInt((float)icm.productResources.all.gold * icm.myBuildings_mags[0].magnfication).ToString();
        productResource.GetChild(1).GetComponent<Text>().text = Mathf.RoundToInt((float)icm.productResources.all.food * icm.myBuildings_mags[1].magnfication).ToString();
        productResource.GetChild(2).GetComponent<Text>().text = Mathf.RoundToInt((float)icm.productResources.all.environment * icm.myBuildings_mags[2].magnfication).ToString();        
    }

    public bool isEnoughResources(DataModules.Cost cost) {
        if (Gold < cost.gold) return false;
        //if (Env < cost.environment) return false;
        if (Food < cost.food) return false;
        return true;
    }

    private void PrimalEnvEfct() {
        //if (Env <= -1100) {
        //    if (!envEfctOn) {
        //        envEfctOn = !envEfctOn;
        //        efct3 = Efct3Second(false);
        //        efct5 = Efct5Second(false);
        //        StartCoroutine(efct3);
        //        StartCoroutine(efct5);
        //    }
        //}
        //else if (Env > -1100) {
        //    if (envEfctOn) {
        //        envEfctOn = !envEfctOn;
        //        StopCoroutine(efct3);
        //        StopCoroutine(efct5);
        //    }
        //}
        //if (Env < 400)
        //    envBonusProduce = 1.0f;
        //if (Env >= 400)
        //    envBonusProduce = 1.1f;
        //if (Env >= 700 && Env < 1100) {
        //    envBonusProduce = 1.25f;
        //    if (envEfctOn) {
        //        envEfctOn = !envEfctOn;
        //        StopCoroutine(efct3);
        //        StopCoroutine(efct5);
        //    }
        //}
        //else if (Env >= 1100) {
        //    if (!envEfctOn) {
        //        envEfctOn = !envEfctOn;
        //        efct3 = Efct3Second(true);
        //        efct5 = Efct5Second(true);
        //        StartCoroutine(efct3);
        //        StartCoroutine(efct5);
        //    }
        //}
        if (Env < -299) {
            if (!envEfctOn) {
                envEfctOn = !envEfctOn;
                efct3 = Efct3Second(false);
                StartCoroutine(efct3);
            }   
        }
        if (Env >= -299 && Env <= 299) {
            if(efct3 != null)
                StopCoroutine(efct3);
        }
        if (Env > 299) {
            if (!envEfctOn) {
                envEfctOn = !envEfctOn;
                efct3 = Efct3Second(true);
                StartCoroutine(efct3);
            }
        }
    }

    private IEnumerator Efct3Second(bool positive) {
        while (!positive) {
            yield return new WaitForSeconds(3.0f);
            Food -= (uint)Mathf.Round(Env / 5);
            Gold += (uint)Mathf.Round(Env / 5);
        }
        while (positive) {
            yield return new WaitForSeconds(3.0f);
            //icm.DamagePlayerCity((int)Mathf.Round((15 / 2)));
            Food += (uint)Mathf.Round(Env / 5);
            Gold -= (uint)Mathf.Round(Env / 5);
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

    private IEnumerator AoutomaticSystem() {
        int time = 300;
        while (playing) {
            yield return new WaitForSeconds(1.0f);
            time--;
            ingameTimer.text = ((int)(time / 60)).ToString() + ":";
            if (((int)(time % 60)) < 10)
                ingameTimer.text += "0";
            ingameTimer.text += ((int)(time % 60)).ToString();

            Gold += (uint)Mathf.Round((float)icm.productResources.all.gold * icm.myBuildings_mags[0].magnfication);
            Food += (uint)Mathf.Round((float)icm.productResources.all.food * icm.myBuildings_mags[1].magnfication);
            if (Env >= -600 && Env <= 600) {
                Env += (int)Mathf.Round((float)icm.productResources.all.environment * icm.myBuildings_mags[2].magnfication);
                Env -= (int)Mathf.Round((icm.productResources.all.gold + icm.productResources.all.food) / time);
                if (Env < -600)
                    Env = -600;
                if (Env > 600)
                    Env = 600;
            }
            scoreManager.AddScore(icm.productResources.all.gold, IngameScoreManager.ScoreType.Product);
            scoreManager.AddScore(icm.productResources.all.food, IngameScoreManager.ScoreType.Product);
            scoreManager.AddScore(icm.productResources.all.environment, IngameScoreManager.ScoreType.Product);

            PrintResource();
        }
    }

    private void TimeController() {

    }
}

/// <summary>
/// Upgrade 관련 처리
/// </summary>
public partial class PlayerController {
    private bool isUpgradeModalActivated = false;
    [Header("- Upgrade")]
    [SerializeField] Text IndustryUpgradeCost;
    [SerializeField] Text FarmUpgradeCost;
    [SerializeField] Text EnvUpgradeCost;
    [SerializeField] Text MilitaryUpgradeCost;
    [SerializeField] Text IndustryLv;
    [SerializeField] Text FarmLv;
    [SerializeField] Text EnvLv;
    [SerializeField] Text MilitaryLv;

    private bool CanUpgrade(Magnification magnification) {
        if (magnification.foodCost > Food) return false;
        if (magnification.goldCost > Gold) return false;
        return true;
    }

    private void Upgrade(Buttons btn) {
        switch (btn) {
            case Buttons.GOLD:
                icm.myBuildings_mags[0].magnfication = icm.myBuildings_mags[0].lv * 1.8f;
                icm.myBuildings_mags[0].lv++;

                Food -= icm.myBuildings_mags[0].foodCost;
                break;
            case Buttons.FOOD:
                icm.myBuildings_mags[1].magnfication = icm.myBuildings_mags[1].lv * 1.8f;
                icm.myBuildings_mags[1].lv++;

                Gold -= icm.myBuildings_mags[1].goldCost;
                break;
            case Buttons.ENVIRONMENT:
                icm.myBuildings_mags[2].magnfication = icm.myBuildings_mags[2].lv * 2.0f;
                icm.myBuildings_mags[2].lv++;

                Food -= icm.myBuildings_mags[2].foodCost;
                Gold -= icm.myBuildings_mags[2].goldCost;
                break;

            case Buttons.MILITARY:
                icm.myBuildings_mags[3].magnfication = icm.myBuildings_mags[3].lv * 1.45f;
                icm.myBuildings_mags[3].lv++;

                Food -= icm.myBuildings_mags[3].foodCost;
                Gold -= icm.myBuildings_mags[3].goldCost;
                break;
        }
        TechLv++;
        UpdateUpgradeCost(btn);
    }

    private void UpdateUpgradeCost(Buttons btn) {
        switch (btn) {
            case Buttons.GOLD:
                uint Industry_FoodCost = (uint)Math.Round(50.0f * Mathf.Pow(1 + icm.myBuildings_mags[0].lv, 1.15f), 0, MidpointRounding.AwayFromZero);

                icm.myBuildings_mags[0].foodCost = Industry_FoodCost;
                icm.myBuildings_mags[0].goldCost = 0;

                IndustryUpgradeCost.text = "비용\n식량 " + icm.myBuildings_mags[0].foodCost;
                IndustryLv.text = "Lv " + icm.myBuildings_mags[0].lv;
                break;
            case Buttons.FOOD:
                uint Farm_GoldCost = (uint)Math.Round(50.0f * Mathf.Pow(1 + icm.myBuildings_mags[1].lv, 1.15f), 0, MidpointRounding.AwayFromZero);

                icm.myBuildings_mags[1].foodCost = 0;
                icm.myBuildings_mags[1].goldCost = Farm_GoldCost;

                FarmUpgradeCost.text = "비용\n골드 " + icm.myBuildings_mags[1].goldCost;
                FarmLv.text = "Lv " + icm.myBuildings_mags[1].lv;
                break;
            case Buttons.ENVIRONMENT:
                uint Env_FoodCost = (uint)Math.Round(50.0f * Mathf.Pow(1 + icm.myBuildings_mags[2].lv, 1.08f), 0, MidpointRounding.AwayFromZero);
                uint Env_GoldCost = Env_FoodCost;

                icm.myBuildings_mags[2].foodCost = Env_FoodCost;
                icm.myBuildings_mags[2].goldCost = Env_GoldCost;

                EnvUpgradeCost.text = "비용\n골드 " + icm.myBuildings_mags[2].goldCost + "\n" + "식량" + icm.myBuildings_mags[2].foodCost;
                EnvLv.text = "Lv " + icm.myBuildings_mags[2].lv;
                break;
            case Buttons.MILITARY:
                uint Military_FoodCost = (uint)Math.Round(50.0f * Mathf.Pow(1 + icm.myBuildings_mags[3].lv, 1.08f), 0, MidpointRounding.AwayFromZero);
                uint Military_GoldCost = Military_FoodCost;

                icm.myBuildings_mags[3].foodCost = Military_FoodCost;
                icm.myBuildings_mags[3].goldCost = Military_GoldCost;

                MilitaryUpgradeCost.text = "비용\n골드 " + icm.myBuildings_mags[3].goldCost + "\n" + "식량" + icm.myBuildings_mags[3].foodCost;
                MilitaryLv.text = "Lv " + icm.myBuildings_mags[3].lv; 
                break;
        }
    }

    private void UpdateUpgradeCost() {
        UpdateUpgradeCost(Buttons.GOLD);
        UpdateUpgradeCost(Buttons.FOOD);
        UpdateUpgradeCost(Buttons.ENVIRONMENT);
        UpdateUpgradeCost(Buttons.MILITARY);
    }

    public void HqUpgrade() {
        Debug.Log("HQ LV : " + hqLevel);
        hqLevel++;
        PrintResource();
        IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, null);
        
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

    public void CloseUpgradeModal() {
        //modal 비활성화
        //scroll 활성화
        isUpgradeModalActivated = false;

        var horizontalScrollSnap = transform.Find("Horizontal Scroll Snap").GetComponent<UnityEngine.UI.Extensions.HorizontalScrollSnap>();
        horizontalScrollSnap.enabled = true;
        var scrollRect = transform.Find("Horizontal Scroll Snap").GetComponent<ScrollRect>();
        scrollRect.enabled = true;

        GameObject uiPanel = transform.Find("UpgradeModal").gameObject;
        uiPanel.SetActive(false);
    }

    public bool isUpgradeModalActivate() {
        return isUpgradeModalActivated;
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