using DataModules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using System;
using Container;
using Sirenix.OdinInspector;
using TMPro;

public partial class PlayerController : SerializedMonoBehaviour {
    public class ProductInfo { //gold food environment 순서의 생산량 저장
        public int[] clickGold;
        public int[] clickFood;
        public int[] clickEnvironment;
    }

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.IngameScene;

    [Header(" - UI")]
    [SerializeField] Transform productResource;
    [SerializeField] Text ingameTimer;

    [Header(" - ResourceText")]
    [SerializeField] Text goldValue;
    [SerializeField] Image goldBar;
    //[SerializeField] Text foodValue;
    //[SerializeField] Text turnValue;
    [SerializeField] Image envValue;
    [SerializeField] IngameCityManager icm;
    
    private bool playing = false;

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

    public ProductInfo pInfo { get; set; }
    IngameScoreManager scoreManager;
    private bool warningOn = false;
    private float envBonusProduce;

    private bool envEfctOn = false;
    private IEnumerator efct3;
    private IEnumerator efct5;

    [Header(" - Player Maps")]
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<Player, GameObject> maps;
    IngameSceneEventHandler eventHandler;

    private static PlayerController _instance;

    public static PlayerController Instance {
        get {
            if (_instance == null) {
                Debug.LogError("PlayerController를 찾을 수 없습니다.");
                return null;
            }
            else {
                return _instance;
            }
        }
    }

    private void Awake() {
        GameObject go = AccountManager.Instance.transform.GetChild(0).GetChild(AccountManager.Instance.leaderIndex).gameObject;

        GameObject ld = (GameObject)Instantiate(
            go,
            maps[Player.PLAYER_1].transform
        );
        TileGroup tileGroup = go.GetComponent<TileGroup>();
        ld.SetActive(true);
        foreach (Transform tile in ld.transform) {
            tile.gameObject.layer = 8;
            foreach (Transform building in tile) {
                building.gameObject.layer = 8;
            }
        }

        for(int i=0; i< go.GetComponent<TileGroup>().units.Count; i++) {
            ld.GetComponent<TileGroup>().units[i].baseSpec.unit.cost = go.GetComponent<TileGroup>().units[i].baseSpec.unit.cost;
        }
        for(int i=0; i<go.GetComponent<TileGroup>().spells.Count; i++) {
            ld.GetComponent<TileGroup>().spells[i].baseSpec.skill.cost = go.GetComponent<TileGroup>().spells[i].baseSpec.skill.cost;
        }

        //TileGroup tmp = ld.GetComponent<TileGroup>();

        ld.transform.localScale = new Vector3(1, 1, 1);
        ld.transform.Find("Background").gameObject.SetActive(false);

        scoreManager = IngameScoreManager.Instance;
        pInfo = new ProductInfo();
        pInfo.clickGold = new int[3];
        pInfo.clickFood = new int[3];
        pInfo.clickEnvironment = new int[3];
        goldBar.fillAmount = 0;

        eventHandler = IngameSceneEventHandler.Instance;
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, OnMyBuildings_info_added);

        _instance = this;
    }

    // Use this for initialization
    void Start() {
        playerActiveCards().Init();
        playerBuildings().Init();
        playerBuildings().RemoveTile();
        
        deckShuffler().InitCard();
        resourceManager().OnGoldProduce(true);
        //PrintResource();
        //PrimalEnvEfct();

        //icm.productResources.gold.gold += icm.upgradeInfos[0].product.gold;
        //icm.productResources.food.food += icm.upgradeInfos[0].product.food;
        //icm.productResources.env.environment += icm.upgradeInfos[0].product.env;

        //coinAni.GetSkeletonData(false);
        //SetPlayerConsumeResource();
        //playing = true;
        //StartCoroutine(AoutomaticSystem());
    }

    void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, OnMyBuildings_info_added);
    }

    private void OnMyBuildings_info_added(Enum Event_Type, Component Sender, object Param) {
        foreach (BuildingInfo buildingInfo in playerBuildings().buildingInfos) {
            playerResource().TotalHp += buildingInfo.maxHp;
        }
        playerResource().maxhp = playerResource().TotalHp;
    }

    //public void PrintResource() {
    //    goldValue.text = Gold.ToString();
    //    foodValue.text = Food.ToString();

    //    Transform envBar = envValue.transform.parent.GetChild(1);
    //    envBar.localPosition = new Vector3(((float)Env / 600.0f) * 540, 0, 0);
    //    Text envText = envValue.transform.parent.GetChild(3).GetComponent<Text>();
    //    envText.text = Env.ToString();
    //    showResource();
    //}

    //private void showResource() {
    //    productResource.GetChild(0).GetComponent<Text>().text = Mathf.RoundToInt((float)icm.productResources.all.gold * icm.myBuildings_mags[0].magnfication).ToString();
    //    productResource.GetChild(1).GetComponent<Text>().text = Mathf.RoundToInt((float)icm.productResources.all.food * icm.myBuildings_mags[1].magnfication).ToString();
    //    productResource.GetChild(2).GetComponent<Text>().text = Mathf.RoundToInt((float)icm.productResources.all.environment * icm.myBuildings_mags[2].magnfication).ToString();        
    //}

    //public bool isEnoughResources(DataModules.Cost cost) {
    //    if (Gold < cost.gold) return false;
    //    if (Food < cost.food) return false;
    //    return true;
    //}

    //private void PrimalEnvEfct() {
    //    if (Env < -299) {
    //        if (!envEfctOn) {
    //            envEfctOn = !envEfctOn;
    //            efct3 = Efct3Second(false);
    //            StartCoroutine(efct3);
    //        }   
    //    }
    //    if (Env >= -299 && Env <= 299) {
    //        if(efct3 != null)
    //            StopCoroutine(efct3);
    //    }
    //    if (Env > 299) {
    //        if (!envEfctOn) {
    //            envEfctOn = !envEfctOn;
    //            efct3 = Efct3Second(true);
    //            StartCoroutine(efct3);
    //        }
    //    }
    //}

    //private IEnumerator Efct3Second(bool positive) {
    //    while (!positive) {
    //        yield return new WaitForSeconds(3.0f);
    //        Food -= (uint)Mathf.Round(Env / 5);
    //        Gold += (uint)Mathf.Round(Env / 5);
    //    }
    //    while (positive) {
    //        yield return new WaitForSeconds(3.0f);
    //        Food += (uint)Mathf.Round(Env / 5);
    //        Gold = CheckResourceFlow(Gold, (uint)Mathf.Round(Env / 5), false);
    //    }
    //}

    //private IEnumerator AoutomaticSystem() {
    //    int time = 300;
    //    while (playing) {
    //        yield return new WaitForSeconds(1.0f);
    //        time--;
    //        ingameTimer.text = ((int)(time / 60)).ToString() + ":";
    //        if (((int)(time % 60)) < 10)
    //            ingameTimer.text += "0";
    //        ingameTimer.text += ((int)(time % 60)).ToString();

    //        Gold += (uint)Mathf.Round((float)icm.productResources.all.gold * icm.myBuildings_mags[0].magnfication);
    //        Food += (uint)Mathf.Round((float)icm.productResources.all.food * icm.myBuildings_mags[1].magnfication);
    //        if (Env >= -600 && Env <= 600) {
    //            Env += (int)Mathf.Round((float)icm.productResources.all.environment * icm.myBuildings_mags[2].magnfication);
    //            Env -= (int)Mathf.Round((icm.productResources.all.gold + icm.productResources.all.food) / time);
    //            if (Env < -600)
    //                Env = -600;
    //            if (Env > 600)
    //                Env = 600;
    //        }
    //        scoreManager.AddScore(icm.productResources.all.gold, IngameScoreManager.ScoreType.Product);
    //        scoreManager.AddScore(icm.productResources.all.food, IngameScoreManager.ScoreType.Product);
    //        scoreManager.AddScore(icm.productResources.all.environment, IngameScoreManager.ScoreType.Product);

    //        PrintResource();
    //        PrimalEnvEfct();
    //    }
    //}
    //public void SetPlayerConsumeResource() {
    //    float hp = icm.cityMaxHP;
    //    MaxHpMulti = Mathf.RoundToInt(hp * 0.005f);
    //    tileCount = icm.CityTotalTileCount();
    //}

    //public uint CheckResourceFlow(uint target, uint am1, bool sum) {
    //    int targetSource, amount;
    //    targetSource = (int)target;
    //    amount = (int)am1;


    //    if (sum == true)
    //        targetSource += amount;
    //    else if (sum == false)
    //        targetSource -= amount;

    //    if (targetSource > 0) {
    //        return (uint)targetSource;
    //    }
    //    else
    //        return 0;
    //}
}

    /// <summary>
    /// 각각의 컨테이너를 리턴
    /// </summary>
    public partial class PlayerController {
    public PlayerResource playerResource() {
        return GetComponent<PlayerResource>();
    }

    public IngameResourceManager resourceManager() {
        return GetComponent<IngameResourceManager>();
    }

    public MyBuildings playerBuildings() {
        return GetComponent<MyBuildings>();
    }

    public PlayerActiveCards playerActiveCards() {
        return GetComponent<PlayerActiveCards>();
    }

    public IngameDeckShuffler deckShuffler() {
        return transform.GetChild(0).GetComponent<IngameDeckShuffler>();
    }
}

public partial class PlayerController {
    public enum Player {
        PLAYER_1,
        PLAYER_2,
        PLAYER_3,
        PLAYER_4
    }
}

/// <summary>
/// (영웅)유닛 관련 처리
/// </summary>
public partial class PlayerController : SerializedMonoBehaviour {
    [Header(" - Prefabs")]
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<string, GameObject> heroPrefabs;

    [SerializeField] Transform summonParent;

    public void HeroSummon(ActiveCard card) {
        var result = GetHeroPrefab(card.baseSpec.unit.id);
        if(result == null) {
            result = GetHeroPrefab("n_uu_01001");
        }

        GameObject hero = Instantiate(result, summonParent);
        UnitAI unitAI = hero.GetComponent<UnitAI>();
        unitAI.SetUnitData(card);

        GameObject name = hero.transform.Find("Name").gameObject;
        name.SetActive(true);
        name.GetComponent<TextMeshPro>().text = card.baseSpec.unit.name;
    }

    public GameObject GetHeroPrefab(string id) {
        if (!heroPrefabs.ContainsKey(id)) return null;
        return heroPrefabs[id];
    }
}