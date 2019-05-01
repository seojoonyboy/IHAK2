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
        StartCoroutine(Stage2AI());
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

    public void HeroSummon(ActiveCard card, GameObject cardObj) {
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
        unitAI.Init(card, cardObj);
        unitAI.gameObject.layer = 11;

        GameObject name = hero.transform.Find("Name").gameObject;
        name.SetActive(true);
        name.GetComponent<TextMeshPro>().text = card.baseSpec.unit.name;
        _instance.GetComponent<MinionSpawnController>().SpawnMinionSquad(card, unitGroup.transform, true);
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

//AI 시나리오
public partial class EnemyPlayerController : SerializedMonoBehaviour {
    [SerializeField] Transform nodeParent;

    UnitGroup racanRobot;
    UnitGroup wimpRobot;

    List<Vector3> racanPath = new List<Vector3>();
    List<Vector3> wimpPath = new List<Vector3>();

    IEnumerator Stage2AI() {
        while (alive) {
            yield return new WaitForSeconds(5.0f);
            HeroSummon(playerctlr.GetComponent<PlayerActiveCards>().opponentCards[0], null);
            racanRobot = summonParent.GetChild(6).GetComponent<UnitGroup>();
            StartCoroutine(RacanDetector());
            HeroSummon(playerctlr.GetComponent<PlayerActiveCards>().opponentCards[1], null);
            wimpRobot = summonParent.GetChild(7).GetComponent<UnitGroup>();
            StartCoroutine(WimpDetector());

            yield return new WaitForSeconds(2.0f);
            racanPath.Add(nodeParent.Find("S12").transform.position);
            racanPath.Add(nodeParent.Find("S20").transform.position);
            wimpPath.Add(nodeParent.Find("S12").transform.position);
            wimpPath.Add(nodeParent.Find("S00").transform.position);
            racanRobot.SetMove(racanPath);
            wimpRobot.SetMove(wimpPath);
            break;
        }
    }

    IEnumerator RacanDetector() {
        while(true) {
            if (racanRobot == null) {
                yield return new WaitForSeconds(2.0f);
                HeroSummon(playerctlr.GetComponent<PlayerActiveCards>().opponentCards[0], null);
                racanRobot = summonParent.GetChild(summonParent.childCount - 1).GetComponent<UnitGroup>();
                racanRobot.SetMove(racanPath);
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator WimpDetector() {
        while (true) {
            if (wimpRobot == null) {
                yield return new WaitForSeconds(2.0f);
                HeroSummon(playerctlr.GetComponent<PlayerActiveCards>().opponentCards[1], null);
                wimpRobot = summonParent.GetChild(summonParent.childCount - 1).GetComponent<UnitGroup>();
                wimpRobot.SetMove(wimpPath);
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

}