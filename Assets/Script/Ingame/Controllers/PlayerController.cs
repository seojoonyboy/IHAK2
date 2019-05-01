using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Container;
using Sirenix.OdinInspector;
using TMPro;
using System.Text;
using ingameUIModules;
using Resources = UnityEngine.Resources;
using UniRx;

public partial class PlayerController : SerializedMonoBehaviour {
    [Header(" - ResourceText")]
    [SerializeField] Image goldBar;
    
    private bool playing = false;
    public bool IsPlaying {
        get { return playing; }
    }
    IngameScoreManager scoreManager;

    [Header(" - Player Maps")]
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<Player, GameObject> maps;

    public MapStation hq_mapStation;
    public Transform pathPrefabsParent;
    public GameObject 
        GoldResourceFlick,
        CitizenResourceFlick;
    Transform myCity;
    public IngameResultManager resultManager;

    public StageGoal stageGoals;

    private static PlayerController _instance;
    IngameSceneEventHandler eventHandler;
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

    public class StageGoal {
        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public Dictionary<string, string> missionLists;
    }

    private void SetMap() {
        maps.Add(Player.PLAYER_1, GameObject.Find("PlayerCity"));
        maps.Add(Player.PLAYER_2, GameObject.Find("EnemyCity"));
        maps.Add(Player.PLAYER_3, GameObject.Find("EnemyCity"));
        maps.Add(Player.PLAYER_4, GameObject.Find("EnemyCity"));

        summonParent = GameObject.Find("PlayerMinionSpawnPos").transform;
        myCity = maps[Player.PLAYER_1].transform;
    }

    private void Awake() {
        SetMap();
        SetHQ();

        GameObject go = AccountManager.Instance
            .transform
            .GetChild(0)
            .GetChild(AccountManager.Instance.leaderIndex)
            .gameObject;
        GameObject ld = Instantiate(
            go,
            maps[Player.PLAYER_1].transform
        );
        GetDeckDetailRequest(ld);

        scoreManager = IngameScoreManager.Instance;
        if (goldBar != null) goldBar.fillAmount = 0;

        eventHandler = IngameSceneEventHandler.Instance;
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.MY_DECK_DETAIL_INFO_ADDED, OnMyDeckInfoAdded);

        hq_mapStation = GameObject.Find("S10").GetComponent<MapStation>();
        _instance = this;

        //StartCoroutine(subgoal_test());
    }

    IEnumerator subgoal_test() {
        yield return new WaitForSeconds(5.0f);
        eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, this, "1-1");
        yield return new WaitForSeconds(3.0f);
        eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, this, "1-2");
        yield return new WaitForSeconds(3.0f);
        eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, this, "1-3");
        yield return new WaitForSeconds(3.0f);
        eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, this, "1-4");
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
        Debug.Log(url);
        _networkManager.request("GET", url.ToString(), GetDetailDeckCallback, false);
    }

    private void GetDetailDeckCallback(HttpResponse response) {
        if (response.responseCode == 200 || response.responseCode == 404) {
            if (response.data != null) {
                //deck = JsonReader.Read<Req_deckDetail.Deck>(response.data.ToString());
                //DeckInfo deckInfo = maps[Player.PLAYER_1]
                //    .transform
                //    .GetChild(0)
                //    .gameObject
                //    .GetComponent<DeckInfo>();

                //foreach(Req_deckDetail.Card card in deck.cards) {
                //    if (card.data.type == "hero") {
                //        ActiveCard activeCard = deckInfo.units.Find(x => x.id == card.id);
                //        if(activeCard != null) {
                //            activeCard.baseSpec.unit.skill = card.data.unit.skill;
                //            activeCard.baseSpec.unit.minion = card.data.unit.minion;
                //        }
                //    }
                //}
                eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.MY_DECK_DETAIL_INFO_ADDED, null);
            }
        }
        else if (response.responseCode == 404) {

        }

    }

    void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.MY_DECK_DETAIL_INFO_ADDED, OnMyDeckInfoAdded);
    }

    private void OnMyDeckInfoAdded(Enum Event_Type, Component Sender, object Param) {
        playing = true;

        playerActiveCards().Init();
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

            if (pair.Key == "Minion_die_gold") {
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

/// <summary>
/// 건물 배치
/// </summary>
public partial class PlayerController : SerializedMonoBehaviour {
    private void SetHQ() {
        GameObject hq = Instantiate(Resources.Load("Prefabs/HQ") as GameObject);
        hq.transform.SetParent(myCity);

        int hp = (int)AccountManager.Instance.mission.hqHitPoint;
        IngameHpSystem.Instance.SetHp(
            Player.PLAYER_1,
            (int)AccountManager.Instance.mission.hqHitPoint
        );

        hq.GetComponent<IngameBuilding>().SetHp(hp);

        hq
            .ObserveEveryValueChanged(x => x.GetComponent<IngameBuilding>().HP)
            .Subscribe(_ => {
                IngameHpSystem.Instance.HpChanged(Player.PLAYER_1, hq.GetComponent<IngameBuilding>().HP);
            });
    }
}