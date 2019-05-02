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
using Resources = UnityEngine.Resources;

public partial class EnemyPlayerController : SerializedMonoBehaviour {
    public class ProductInfo { //gold food environment 순서의 생산량 저장
        public int[] clickGold;
        public int[] clickFood;
        public int[] clickEnvironment;
    }

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.IngameScene;
    private PlayerController playerctrl;

    private bool alive = true;
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
    [SerializeField] PlayerController playerctlr;


    [Header(" - Player Maps")]
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<PlayerController.Player, GameObject> maps;
    IngameSceneEventHandler eventHandler;
    Req_deckDetail.Deck deck;
    public MapStation hq_mapStation;
    public Transform pathPrefabsParent;
    Transform myCity;

    private static EnemyPlayerController _instance;

    public static EnemyPlayerController Instance {
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

    private void SetMap() {
        maps.Add(PlayerController.Player.PLAYER_1, GameObject.Find("PlayerCity"));
        maps.Add(PlayerController.Player.PLAYER_2, GameObject.Find("EnemyCity"));
        maps.Add(PlayerController.Player.PLAYER_3, GameObject.Find("EnemyCity"));
        maps.Add(PlayerController.Player.PLAYER_4, GameObject.Find("EnemyCity"));

        summonParent = GameObject.Find("EnemyMinionSpawnPos").transform;
        myCity = PlayerController.Instance.maps[PlayerController.Player.PLAYER_2].transform;
    }

    private void Awake() {
        SetMap();
        SetHQ();    //HQ 건물 추가

        eventHandler = IngameSceneEventHandler.Instance;
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.MY_DECK_DETAIL_INFO_ADDED, OnMyDeckInfoAdded);

        hq_mapStation = GameObject.Find("S12").GetComponent<MapStation>();
        _instance = this;
    }

    private void Start() {
        nodeParent = GameObject.Find("Nodes").transform;
        groups = new UnitGroup[4];
        switch (AccountManager.Instance.mission.stageNum) {
            case 2:
                StartCoroutine(Stage2AI());
                break;
            case 3:
                StartCoroutine(Stage3AI());
                break;
            default:
                break;
        }
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
        if (response.responseCode == 200) {
            if (response.data != null) {
                deck = JsonReader.Read<Req_deckDetail.Deck>(response.data.ToString());
                DeckInfo deckInfo = maps[PlayerController.Player.PLAYER_1]
                    .transform
                    .GetChild(0)
                    .gameObject
                    .GetComponent<DeckInfo>();

                foreach (Req_deckDetail.Card card in deck.cards) {
                    if (card.data.type == "hero") {
                        ActiveCard activeCard = deckInfo.units.Find(x => x.id == card.id);
                        if (activeCard != null) {
                            activeCard.baseSpec.unit.skill = card.data.unit.skill;
                            activeCard.baseSpec.unit.minion = card.data.unit.minion;
                        }
                    }
                }
                //Player의 덱을 불러오는 용도로 설계되어 있어 적에 의한 요청과 구분이 필요함
                //eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.MY_DECK_DETAIL_INFO_ADDED, null);
            }
        }
    }

    void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.MY_DECK_DETAIL_INFO_ADDED, OnMyDeckInfoAdded);
    }

    private void OnMyDeckInfoAdded(Enum Event_Type, Component Sender, object Param) {
        playing = true;

        resourceManager().OnResourceProduce(true);
    }
}

/// <summary>
/// 각각의 컨테이너를 리턴
/// </summary>
public partial class EnemyPlayerController {
    public PlayerResource playerResource() {
        return _instance.GetComponent<PlayerResource>();
    }

    public IngameResourceManager resourceManager() {
        return _instance.GetComponent<IngameResourceManager>();
    }

    public MinionSpawnController MinionSpawnController() {
        return _instance.GetComponent<MinionSpawnController>();
    }
}


/// <summary>
/// (영웅)유닛 관련 처리
/// </summary>
public partial class EnemyPlayerController : SerializedMonoBehaviour {
    [Header(" - Prefabs")]
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<string, GameObject> heroPrefabs;

    [SerializeField] GameObject unitGroupPrefab;
    [SerializeField] Transform summonParent;

    public void HeroSummon(ActiveCard card, GameObject cardObj, int minionNum = 0) {
        GameObject unitGroup = Instantiate(unitGroupPrefab, summonParent);
        var result = GetHeroPrefab(card.baseSpec.unit.id);
        if (result == null) {
            result = GetHeroPrefab("n_uu_01001");
        }

        GameObject hero = Instantiate(result, unitGroup.transform);
        //cardObj.GetComponent<HeroCardHandler>().instantiatedUnitObj = hero;

        UnitAI unitAI = hero.GetComponent<UnitAI>();
        //hero.GetComponent<HeroAI>().targetCard = cardObj;

        unitAI.ownerNum = PlayerController.Player.PLAYER_2;
        unitAI.gameObject.layer = 11;
        unitAI.Init(card, cardObj);

        GameObject name = hero.transform.Find("Name").gameObject;
        name.SetActive(true);
        name.GetComponent<TextMeshPro>().text = card.baseSpec.unit.name;
        if (mission2on)
            _instance.GetComponent<MinionSpawnController>().SpawnMinionSquad(card, unitGroup.transform, true);
        if (mission3on)
            _instance.GetComponent<MinionSpawnController>().SpawnMinionSquad(card, unitGroup.transform, minionNum);
        //GetComponent<UnitGroup>().SetMove(cardObj.GetComponent<HeroCardHandler>().path);
    }

    public GameObject GetHeroPrefab(string id) {
        if (!heroPrefabs.ContainsKey(id)) return null;
        return heroPrefabs[id];
    }
}

/// <summary>
/// 건물 배치
/// </summary>
public partial class EnemyPlayerController : SerializedMonoBehaviour {
    private void SetHQ() {
        GameObject hq = Instantiate(Resources.Load("Prefabs/HQ") as GameObject);
        hq.transform.SetParent(myCity);

        int hp = (int)AccountManager.Instance.mission.hqHitPoint;
        IngameHpSystem.Instance.SetHp(
            PlayerController.Player.PLAYER_2,
            hp
        );

        hq.GetComponent<IngameBuilding>().SetHp(hp);

        hq
            .ObserveEveryValueChanged(x => x.GetComponent<IngameBuilding>().HP)
            .Subscribe(_ => {
                IngameHpSystem.Instance.HpChanged(PlayerController.Player.PLAYER_2, hq.GetComponent<IngameBuilding>().HP);
            });
    }

}

public partial class EnemyPlayerController : SerializedMonoBehaviour {
    [Header(" - MissionData")]
    [SerializeField] Transform nodeParent;

    bool mission2on = false;
    bool mission3on = false;

    UnitGroup[] groups; //1:라칸, 2:윔프, 3:쉘, 4:렉스 

    List<Vector3> racanPath = new List<Vector3>();
    List<Vector3> wimpPath = new List<Vector3>();
    List<Vector3> rexPath = new List<Vector3>();
    List<Vector3> shellPath = new List<Vector3>();
}

public partial class EnemyPlayerController : SerializedMonoBehaviour {

    IEnumerator Stage2AI() {
        mission2on = true;
        yield return new WaitForSeconds(5.0f);
        HeroSummon(playerctlr.GetComponent<PlayerActiveCards>().opponentCards[0], null);
        groups[0] = summonParent.GetChild(6).GetComponent<UnitGroup>();
        StartCoroutine(RacanDetector());
        HeroSummon(playerctlr.GetComponent<PlayerActiveCards>().opponentCards[1], null);
        groups[1] = summonParent.GetChild(7).GetComponent<UnitGroup>();
        StartCoroutine(WimpDetector());
        StartCoroutine(RexDetector());
        StartCoroutine(StationDetector());

        yield return new WaitForSeconds(2.0f);
        racanPath.Add(nodeParent.Find("S12").transform.position);
        racanPath.Add(nodeParent.Find("S20").transform.position);
        wimpPath.Add(nodeParent.Find("S12").transform.position);
        wimpPath.Add(nodeParent.Find("S00").transform.position);
        groups[0].SetMove(racanPath);
        groups[1].SetMove(wimpPath);
    }

    IEnumerator RacanDetector() {
        while (true) {
            if (!mission2on) break;
            if (SearchLevelUp()) break;
            if (groups[0] == null) {
                yield return new WaitForSeconds(2.0f);
                HeroSummon(playerctlr.GetComponent<PlayerActiveCards>().opponentCards[0], null);
                groups[0] = summonParent.GetChild(summonParent.childCount - 1).GetComponent<UnitGroup>();
                StartCoroutine(RacanDetector());
                groups[0].SetMove(racanPath);
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator WimpDetector() {
        while (true) {
            if (!mission2on) break;
            if (SearchLevelUp()) break;
            if (groups[1] == null) {
                yield return new WaitForSeconds(2.0f);
                HeroSummon(playerctlr.GetComponent<PlayerActiveCards>().opponentCards[1], null);
                groups[1] = summonParent.GetChild(summonParent.childCount - 1).GetComponent<UnitGroup>();
                StartCoroutine(WimpDetector());
                groups[1].SetMove(wimpPath);
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator RexDetector() {
        while (true) {
            if (!PlayerController.Instance.FirstMove)
                yield return new WaitForSeconds(0.1f);
            else {
                HeroSummon(playerctlr.GetComponent<PlayerActiveCards>().opponentCards[2], null);
                break;
            }
        }
    }

    IEnumerator StationDetector() {
        while (true) {
            if (nodeParent.GetChild(0).GetComponent<DefaultStation>().OwnerNum == PlayerController.Player.PLAYER_1
                && nodeParent.GetChild(3).GetComponent<DefaultStation>().OwnerNum == PlayerController.Player.PLAYER_1)
                mission2on = false;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private bool SearchLevelUp() {
        foreach (ActiveCard card in playerctlr.GetComponent<PlayerActiveCards>().activeCards) {
            if (card.ev.lv > 1) {
                return true;
            }
        }
        return false;
    }

}


public partial class EnemyPlayerController : SerializedMonoBehaviour {
    private int aiMana = 0;
    private int aiCitizen = 0;
    IEnumerator[] m3unitCtrl = new IEnumerator[4];

    public int AiCitizen {
        get { return aiCitizen; }
        set { aiCitizen = value; }
    }

    IEnumerator Stage3AI() {
        mission3on = true;
        m3unitCtrl[0] = RacanAI();
        m3unitCtrl[1] = WimpAI();
        m3unitCtrl[2] = ShellAI();
        m3unitCtrl[3] = RexAI();
        StartCoroutine(ProduceMana());
        StartCoroutine(ProduceCitizen());
        while (true) {
            yield return new WaitForSeconds(1.0f);
            int heroIndex = UnityEngine.Random.Range(0, 4);
            if (groups[heroIndex] == null) {
                if (playerctlr.GetComponent<PlayerActiveCards>().opponentCards[heroIndex].baseSpec.unit.cost.gold < aiMana) {
                    int spwanCitizen = 0;
                    int reqCitizen = (int)playerctlr.GetComponent<PlayerActiveCards>().opponentCards[heroIndex].baseSpec.unit.cost.population;
                    if (reqCitizen > aiCitizen)
                        spwanCitizen = aiCitizen;
                    else
                        spwanCitizen = reqCitizen;
                    HeroSummon(playerctlr.GetComponent<PlayerActiveCards>().opponentCards[heroIndex], null, spwanCitizen);
                    groups[heroIndex] = summonParent.GetChild(summonParent.childCount - 1).GetComponent<UnitGroup>();
                    StartCoroutine(m3unitCtrl[heroIndex]);
                }
            }
        }

    }

    IEnumerator ProduceMana() {
        yield return new WaitForSeconds(2.0f);
        if (aiMana < 10) aiMana++;
    }

    IEnumerator ProduceCitizen() {
        yield return new WaitForSeconds(12.5f);
        if (aiMana < 10) aiCitizen++;
    }

    IEnumerator RacanAI() {
        while (true) {
            if (!mission3on) break;
            if (groups[0] == null) break;
            yield return new WaitForSeconds(0.1f);
            MapStation.NodeDirection wayNum = (MapStation.NodeDirection)UnityEngine.Random.Range(0, 8);
            int count = 0;
            while (true) {
                if (groups[0].currentStation.adjNodes.ContainsKey(wayNum)) {
                    if (count < 20 && groups[0].currentStation.adjNodes[wayNum].gameObject.layer != 11) {
                        racanPath.Add(groups[0].currentStation.transform.position);
                        racanPath.Add(groups[0].currentStation.adjNodes[wayNum].transform.position);
                        groups[0].SetMove(racanPath);
                    }
                }
                count++;
            }
        }
    }
    IEnumerator WimpAI() {
        while (true) {
            if (!mission3on) break;
            if (groups[1] == null) break;
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator ShellAI() {
        while (true) {
            if (!mission3on) break;
            if (groups[2] == null) break;
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator RexAI() {
        while (true) {
            if (!mission3on) break;
            if (groups[3] == null) break;
            yield return new WaitForSeconds(0.1f);
        }
    }
}