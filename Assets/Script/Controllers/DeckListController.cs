using UnityEngine;
using UnityEngine.UI;
using DataModules;

using System.Collections;
using System.Collections.Generic;

using UniRx;
using UniRx.Triggers;
using System;
using System.Text;

using System.Linq;

public partial class DeckListController : MonoBehaviour {
    private NetworkManager _networkManager;
    private AccountManager accountManager;

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.MenuScene;
    MenuSceneEventHandler eventHandler;

    [SerializeField] private GameObject[] slots;
    private List<GameObject> items;
    public GameObject
        Add,
        Modify;

    public GameObject temp;
    [SerializeField] GameObject speciesSelModal;

    private void Awake() {
        _networkManager = NetworkManager.Instance;
        accountManager = AccountManager.Instance;
    }

    void Start() {
        eventHandler = MenuSceneEventHandler.Instance;
        eventHandler.AddListener(MenuSceneEventHandler.EVENT_TYPE.INITIALIZE_DECK, Initialize);
    }

    private void OnDestroy() {
        eventHandler.RemoveListener(MenuSceneEventHandler.EVENT_TYPE.INITIALIZE_DECK, Initialize);
    }

    /// <summary>
    /// 내 덱 정보를 서버에 요청함
    /// 요청 결과로 AccountManager의 decks 갱신
    /// 각 deck마다 (decks의) TileGroup에 총 생산력 정보, Tile마다 건물 GameObject 배치
    /// </summary>
    /// <param name="Event_Type"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    private void Initialize(Enum Event_Type, Component Sender, object Param) {
        foreach(Transform deckGroup in accountManager.transform) {
            Destroy(deckGroup.gameObject);
        }
        if(accountManager.deckGroup != null) {
            Instantiate(accountManager.deckGroup, accountManager.transform);
        }
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/" + accountManager.DEVICEID + "/decks");
        _networkManager.request("GET", url.ToString(), ReqGetDecksCallback, false);
    }

    private void ReqGetDecksCallback(HttpResponse response) {
        if(response.responseCode == 200) {
            if(response.data != null) {
                List<Deck> decks = JsonReader.Read<List<Deck>>(response.data.ToString());
                accountManager.decks = decks;
                SetBuildingsInTileGroupTiles(decks);

                int leaderIndex = 0;
                foreach (Deck deck in decks) {
                    if (deck.isRepresent) {
                        IngameSceneUIController.deckId = deck.id;
                        break;
                    }
                    leaderIndex++;
                }
                accountManager.leaderIndex = leaderIndex;
                Sort(decks);
                eventHandler.PostNotification(MenuSceneEventHandler.EVENT_TYPE.INITIALIZE_DECK_FINISHED, null, leaderIndex);
            }
        }
    }

    public int GetDeckId(int slotNum) {
        int id = -1;
        if (slotNum <= slots.Length) {
            try {
                id = slots[slotNum].transform.GetChild(0).transform.GetComponent<Index>().Id;
            }
            catch(NullReferenceException e) {

            }
        }
        return id;
    }

    //public void Initialize() {
    //    List<Deck> decks = AccountManager.Instance.decks;
    //    int leaderIndex = 0;
    //    foreach (Deck deck in decks) {
    //        if(deck.isRepresent == true) {
    //            AccountManager.Instance.leaderIndex = leaderIndex;
    //            eventHandler.PostNotification(MenuSceneEventHandler.EVENT_TYPE.CHANGE_MAINSCENE_TILE_GROUP, null, leaderIndex);
    //            break;
    //        }
    //        leaderIndex++;
    //    }
    //    Sort(decks);
    //    AccountManager.Instance.GetDeckDetail(GetDeckId(leaderIndex));
    //    //Debug.Log(GetDeckId(leaderIndex));
    //}

    private void Sort(List<Deck> decks) {
        Clear();

        items = new List<GameObject>();

        for (int i = 0; i < slots.Length; i++) {
            if (slots[i] == null) return;
            if (decks.Count == 0 || decks.Count - 1 < i) break;
            GameObject newItem = Instantiate(Modify, slots[i].transform);
            newItem.transform.Find("Name").GetComponent<Text>().text = decks[i].name;

            //if (i == 0) newItem.transform.Find("LeaderSetBtn/IsLeader").gameObject.SetActive(true);
            if (i == AccountManager.Instance.leaderIndex) newItem.transform.Find("LeaderSetBtn/IsLeader").gameObject.SetActive(true);
            else newItem.transform.Find("LeaderSetBtn/IsLeader").gameObject.SetActive(false);

            int id = decks[i].id;
            newItem.GetComponent<Index>().Id = id;
            newItem.transform.Find("DeleteBtn").GetComponent<Button>().onClick
                .AsObservable()
                .Subscribe(_ => {
                    Modal.instantiate((AccountManager.Instance.FindDeck(id)).name + "덱을 삭제하겠습니까?", Modal.Type.YESNO, () => {
                        AccountManager.Instance.RemoveDeck(id, newItem);
                    });
                });
            newItem.transform.Find("LeaderSetBtn").GetComponent<Button>().onClick
                .AsObservable()
                .Subscribe(_ => {
                    Modal.instantiate((AccountManager.Instance.FindDeck(id)).name + "덱을 대표 덱으로\n설정하시겠습니까?", Modal.Type.YESNO, () => {
                        int index = newItem.transform.Find("LeaderSetBtn").parent.parent.GetSiblingIndex();
                        //AccountManager.Instance.ChangeLeaderDeck(id);
                        AccountManager.Instance.ChangeLeaderDeck(id);
                        Sort(AccountManager.Instance.decks);
                    });
                });
            newItem.transform.Find("ModifyBtn").GetComponent<Button>().onClick
                .AsObservable()
                .Subscribe(_ => {
                    moveToDeckSetting(decks.Find(x => x.id == id));
                    AccountManager.Instance.selectNumber = newItem.transform.parent.GetComponent<Index>().Id;
                });
            items.Add(newItem);
        }
        for (int i = decks.Count; i < slots.Length; i++) {
            if (slots[i] == null) break;
            GameObject newItem = Instantiate(Add, slots[i].transform);
            items.Add(newItem);
            newItem.GetComponent<Button>().onClick.AsObservable().Subscribe(_ => {
                AccountManager.Instance.selectNumber = AccountManager.Instance.decks.Count;
                AccountManager.Instance.SetHQ(AccountManager.Instance.selectNumber);

                speciesSelModal.GetComponent<SpeciesSelectController>().ToggleModal(true);
            });
        }
    }

    private void Clear() {
        foreach (GameObject slot in slots) {
            if (slot == null) return;
            foreach (Transform tf in slot.transform) {
                Destroy(tf.gameObject);
            }
        }
    }

    public void moveToDeckSetting(Deck building = null) {
        GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
        gsm.startScene(sceneState, GameSceneManager.SceneState.DeckSettingScene);
        DeckSettingController.prevData = building;
    }
}

/// <summary>
/// 각 Deck별 세부 처리
/// </summary>
public partial class DeckListController {
    /// <summary>
    /// DeckGroup별 건물 GameObject 생성 및 배치
    /// </summary>
    public void SetBuildingsInTileGroupTiles(List<Deck> data) {
        var decks = accountManager.decks;
        if (decks == null) return;

        ConstructManager cm = ConstructManager.Instance;
        GameObject constructManager = cm.transform.gameObject;
        GameObject targetTile;
        GameObject targetBuilding;

        for (int i = 0; i < decks.Count; i++) {
            TileGroup tileGroup = accountManager.transform.GetChild(0).GetChild(i).GetComponent<TileGroup>();
            int tileCount = accountManager.transform.GetChild(0).GetChild(i).childCount - 1;
            tileGroup.touchPerProdPower = data[i].productResources;

            for (int j = 0; j < tileCount; j++) {
                targetTile = accountManager.transform.GetChild(0).GetChild(i).GetChild(j).gameObject;
                foreach(Transform prevBuilding in targetTile.transform) {
                    Destroy(prevBuilding.gameObject);
                }
                //HQ 설정
                if (j == tileCount / 2) {
                    targetBuilding = FindObjectOfType<ConstructManager>().townCenter;
                    if (targetBuilding != null && targetTile.transform.childCount == 0) {
                        GameObject setBuild = Instantiate(targetBuilding, targetTile.transform);
                        targetTile.GetComponent<TileObject>().buildingSet = true;
                        setBuild.transform.position = targetTile.transform.position;
                        setBuild.GetComponent<BuildingObject>().setTileLocation = targetTile.GetComponent<TileObject>().tileNum;
                        if (setBuild.GetComponent<SpriteRenderer>() != null) {
                            setBuild.GetComponent<SpriteRenderer>().sprite = setBuild.GetComponent<BuildingObject>().mainSprite;
                            setBuild.GetComponent<SpriteRenderer>().sortingOrder = tileCount * 2 - targetTile.GetComponent<TileObject>().tileNum;
                        }
                        else {
                            setBuild.GetComponent<MeshRenderer>().sortingOrder = tileCount * 2 - targetTile.GetComponent<TileObject>().tileNum;
                        }
                    }
                    continue;
                }
                targetBuilding = FindBuildingWithID(decks[i].coordsSerial[j]);
                //그 외
                if (targetBuilding != null) {

                    GameObject setBuild = Instantiate(targetBuilding, targetTile.transform);

                    targetTile.GetComponent<TileObject>().buildingSet = true;
                    setBuild.transform.position = targetTile.transform.position;

                    BuildingObject buildingObject = setBuild.GetComponent<BuildingObject>();
                    buildingObject.setTileLocation = targetTile.GetComponent<TileObject>().tileNum;
                    if (setBuild.GetComponent<SpriteRenderer>() != null) {
                        setBuild.GetComponent<SpriteRenderer>().sprite = setBuild.GetComponent<BuildingObject>().mainSprite;
                        setBuild.GetComponent<SpriteRenderer>().sortingOrder = tileCount * 2 - targetTile.GetComponent<TileObject>().tileNum;
                    }
                    else {
                        setBuild.GetComponent<MeshRenderer>().sortingOrder = tileCount * 2 - targetTile.GetComponent<TileObject>().tileNum;
                    }
                    CardData card = buildingObject.card.data;

                    if (!string.IsNullOrEmpty(card.unit.name)) {
                        ActiveCard activeCard = new ActiveCard();
                        activeCard.parentBuilding = targetTile.transform.GetChild(0).gameObject;
                        activeCard.baseSpec.unit = card.unit;
                        tileGroup.units.Add(activeCard);
                    }

                    if(card.activeSkills.Length != 0) {
                        foreach(Skill skill in card.activeSkills) {
                            ActiveCard activeCard = new ActiveCard();
                            activeCard.parentBuilding = targetTile.transform.GetChild(0).gameObject;
                            activeCard.baseSpec.skill = skill;
                            tileGroup.spells.Add(activeCard);
                        }
                    }
                }
            }
        }
    }

    public GameObject FindBuildingWithID(int ID) {

        GameObject buildingGroup = FindObjectOfType<ConstructManager>().transform.GetChild(0).gameObject;
        GameObject targetBuilding;

        for (int i = 0; i < buildingGroup.transform.childCount; i++) {
            if (buildingGroup.transform.GetChild(i).GetComponent<BuildingObject>().card.id == ID) {
                targetBuilding = buildingGroup.transform.GetChild(i).gameObject;
                return targetBuilding;
            }
        }
        return null;
    }
}