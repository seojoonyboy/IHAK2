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
using System.Text;
using ingameUIModules;

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
    
    private bool playing = false;
    public bool IsPlaying {
        get { return playing; }
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
    Req_deckDetail.Deck deck;
    public GameObject cam;
    public MapStation hq_mapStation;
    public Transform pathPrefabsParent;
    public GameObject 
        GoldResourceFlick,
        CitizenResourceFlick;

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
        DeckInfo deckInfo = go.GetComponent<DeckInfo>();
        ld.SetActive(true);
        foreach (Transform tile in ld.transform) {
            tile.gameObject.layer = 8;
            foreach (Transform building in tile) {
                building.gameObject.layer = 8;
            }
        }

        for(int i=0; i< go.GetComponent<DeckInfo>().units.Count; i++) {
            ld.GetComponent<DeckInfo>().units[i].baseSpec.unit.cost = go.GetComponent<DeckInfo>().units[i].baseSpec.unit.cost;
        }
        for(int i=0; i<go.GetComponent<DeckInfo>().spells.Count; i++) {
            ld.GetComponent<DeckInfo>().spells[i].baseSpec.skill.cost = go.GetComponent<DeckInfo>().spells[i].baseSpec.skill.cost;
        }

        scoreManager = IngameScoreManager.Instance;
        pInfo = new ProductInfo();
        pInfo.clickGold = new int[3];
        pInfo.clickFood = new int[3];
        pInfo.clickEnvironment = new int[3];
        goldBar.fillAmount = 0;

        eventHandler = IngameSceneEventHandler.Instance;
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, OnMyBuildings_info_added);
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.MY_DECK_DETAIL_INFO_ADDED, OnMyDeckInfoAdded);

        GetDeckDetailRequest(ld);
        _instance = this;
    }

    private void GetDeckDetailRequest(GameObject ld) {
        StringBuilder url = new StringBuilder();
        NetworkManager _networkManager = NetworkManager.Instance;
        int id = ld.GetComponent<Index>().Id;

        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/")
            .Append(AccountManager.Instance.DEVICEID)
            .Append("/decks/")
            .Append(id.ToString());
        _networkManager.request("GET", url.ToString(), GetDetailDeckCallback, false);
    }

    private void GetDetailDeckCallback(HttpResponse response) {
        if (response.responseCode == 200) {
            if (response.data != null) {
                deck = JsonReader.Read<Req_deckDetail.Deck>(response.data.ToString());
                DeckInfo deckInfo = maps[Player.PLAYER_1]
                    .transform
                    .GetChild(0)
                    .gameObject
                    .GetComponent<DeckInfo>();

                foreach(Req_deckDetail.Card card in deck.cards) {
                    if (card.data.type == "hero") {
                        ActiveCard activeCard = deckInfo.units.Find(x => x.id == card.id);
                        if(activeCard != null) {
                            activeCard.baseSpec.unit.skill = card.data.unit.skill;
                            activeCard.baseSpec.unit.minion = card.data.unit.minion;
                        }
                    }
                }
                eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.MY_DECK_DETAIL_INFO_ADDED, null);
            }
        }
    }

    void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, OnMyBuildings_info_added);
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.MY_DECK_DETAIL_INFO_ADDED, OnMyDeckInfoAdded);
    }

    private void OnMyBuildings_info_added(Enum Event_Type, Component Sender, object Param) {
        playerResource().maxhp = playerResource().TotalHp = 1000;
    }

    private void OnMyDeckInfoAdded(Enum Event_Type, Component Sender, object Param) {
        playing = true;
        playerActiveCards().Init();
        playerBuildings().Init();
        playerBuildings().RemoveTile();

        deckShuffler().InitCard();
        resourceManager().OnResourceProduce(true);
    }
}

    /// <summary>
    /// 각각의 컨테이너를 리턴
    /// </summary>
    public partial class PlayerController {
    public PlayerResource playerResource() {
        return _instance.GetComponent<PlayerResource>();
    }

    public IngameResourceManager resourceManager() {
        return _instance.GetComponent<IngameResourceManager>();
    }

    public MyBuildings playerBuildings() {
        return _instance.GetComponent<MyBuildings>();
    }

    public PlayerActiveCards playerActiveCards() {
        return _instance.GetComponent<PlayerActiveCards>();
    }

    public IngameDeckShuffler deckShuffler() {
        return _instance.transform.GetChild(0).GetComponent<IngameDeckShuffler>();
    }

    public PlayerPassiveCards PlayerPassiveCards() {
        return _instance.GetComponent<PlayerPassiveCards>();
    }

    public CitizenSpawnController CitizenSpawnController() {
        return _instance.GetComponent<CitizenSpawnController>();
    }

    public MinionSpawnController MinionSpawnController() {
        return _instance.GetComponent<MinionSpawnController>();
    }

    public HeroSummonListener HeroSummonListener() {
        return _instance.GetComponent<HeroSummonListener>();
    }

    public MissionConditionsController MissionConditionsController() {
        return _instance.gameObject.transform.parent.GetComponent<MissionConditionsController>();
    }
}

public partial class PlayerController {
    public enum Player {
        PLAYER_1 = 10,
        PLAYER_2 = 11,
        PLAYER_3 = 12,
        PLAYER_4 = 13,
        NEUTRAL = 14,
    }
}

/// <summary>
/// (영웅)유닛 관련 처리
/// </summary>
public partial class PlayerController : SerializedMonoBehaviour {
    [Header(" - Prefabs")]
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<string, GameObject> heroPrefabs;

    [SerializeField] GameObject unitGroupPrefab;
    [SerializeField] Transform summonParent;
    [SerializeField] Transform passiveUIParent;
    [SerializeField] public Transform spellPrefabParent;

    [SerializeField] GameObject passiveUIPref;

    public void HeroSummon(ActiveCard card, GameObject cardObj) {
        GameObject unitGroup = Instantiate(unitGroupPrefab, summonParent);
        var result = GetHeroPrefab(card.baseSpec.unit.id);
        if(result == null) {
            result = GetHeroPrefab("n_uu_01001");
        }

        GameObject hero = Instantiate(result, unitGroup.transform);
        cardObj.GetComponent<HeroCardHandler>().instantiatedUnitObj = hero;

        UnitAI unitAI = hero.GetComponent<UnitAI>();
        hero.GetComponent<HeroAI>().targetCard = cardObj;

        unitAI.ownerNum = Player.PLAYER_1;
        unitAI.Init(card, cardObj);

        GameObject name = hero.transform.Find("Name").gameObject;
        name.SetActive(true);
        name.GetComponent<TextMeshPro>().text = card.baseSpec.unit.name;
        _instance.GetComponent<MinionSpawnController>().SpawnMinionSquad(card, unitGroup.transform);
        unitGroup.GetComponent<UnitGroup>().SetMove(cardObj.GetComponent<HeroCardHandler>().path);
    }

    public GameObject GetHeroPrefab(string id) {
        if (!heroPrefabs.ContainsKey(id)) return null;
        return heroPrefabs[id];
    }

    public void SetPassiveUI() {
        foreach (KeyValuePair<string, float> pair in PlayerPassiveCards().effectModules) {
            GameObject uiObj = Instantiate(passiveUIPref, passiveUIParent);
            Text desc = uiObj.transform.Find("Text").GetComponent<Text>();

            if (pair.Key == "Unit_health") {
                desc.text = "유닛 체력 버프 +" + pair.Value;
            }

            if(pair.Key == "Minion_die_gold") {
                desc.text = "미니언 사망시 골드 획득 +" + pair.Value;
            }
        }
    }

    public void DieEffect(object instance) {
        Type minionType = typeof(MinionAI);
        if(instance.GetType() == minionType) {
            if (PlayerPassiveCards().effectModules.ContainsKey("Minion_die_gold")) {
                playerResource().Gold += (decimal)PlayerPassiveCards().effectModules["Minion_die_gold"];
            }
        }
    }
}