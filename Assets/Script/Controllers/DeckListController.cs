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
    [SerializeField] GameObject deckModifyBtn;
    [SerializeField] GameObject leaderSetBtn;

    public Transform tg_clone_tar;

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
            catch(NullReferenceException e) { }
        }
        return id;
    }

    private void Sort(List<Deck> decks) {
        Clear();

        items = new List<GameObject>();

        for (int i = 0; i < slots.Length; i++) {
            if (slots[i] == null) return;
            if (decks.Count == 0 || decks.Count - 1 < i) break;
            GameObject newItem = Instantiate(Modify, slots[i].transform);
            newItem.transform.Find("Name").GetComponent<Text>().text = decks[i].name;

            int id = decks[i].id;
            newItem.GetComponent<Index>().Id = id;
            newItem.GetComponent<Button>()
                .onClick
                .AsObservable()
                .Subscribe(_ => OnClickSlot(newItem));
            items.Add(newItem);
        }
        for (int i = decks.Count; i < slots.Length; i++) {
            if (slots[i] == null) break;
            GameObject newItem = Instantiate(Add, slots[i].transform);
            items.Add(newItem);
            newItem.GetComponent<Button>()
                .onClick
                .AsObservable()
                .Subscribe(_ => {
                    OnClickSlot(newItem);
                    accountManager.selectNumber = accountManager.decks.Count;
            });
        }

        GetComponent<Index>().Id = 0;
        if(decks.Count != 0) OnClickSlot(slots[0].gameObject.transform.GetChild(0).gameObject);

        tg_clone_tar.GetChild(0).GetChild(0).gameObject.SetActive(true);
    }

    public void OnClickSlot(GameObject pref) {
        if (pref.name.Contains("Add")) {
            AccountManager.Instance.selectNumber = AccountManager.Instance.decks.Count;
            AccountManager.Instance.SetHQ(AccountManager.Instance.selectNumber);

            speciesSelModal.GetComponent<SpeciesSelectController>().ToggleModal(true);

            return;
        }
        GetComponent<Index>().Id = pref.transform.parent.GetComponent<Index>().Id;
        int leaderIndex = AccountManager.Instance.leaderIndex;
        int slotIndex = pref.transform.parent.GetComponent<Index>().Id;

        foreach(Transform tf in tg_clone_tar.GetChild(0)) {
            tf.gameObject.SetActive(false);
        }

        tg_clone_tar.GetChild(0).GetChild(GetComponent<Index>().Id).gameObject.SetActive(true);

        //Toggle UI
        if (slotIndex == leaderIndex) leaderSetBtn.transform.Find("Slot/Check").gameObject.SetActive(true);
        else leaderSetBtn.transform.Find("Slot/Check").gameObject.SetActive(false);

        for(int i=0; i<accountManager.decks.Count; i++) {
            if(slots[i].GetComponent<Index>().Id == GetComponent<Index>().Id) {
                slots[i].transform.GetChild(0).transform.Find("Selected").gameObject.SetActive(true);
            }
            else {
                slots[i].transform.GetChild(0).transform.Find("Selected").gameObject.SetActive(false);
            }
        }
    }

    public void OnModifyBtn() {
        int selectedIndex = GetComponent<Index>().Id;
        int deckIndex = slots[selectedIndex].transform.GetChild(0).GetComponent<Index>().Id;
        Deck deck = accountManager.decks.Find(x => x.id == deckIndex);
        if (deck == null) return;
        moveToDeckSetting(deck);
    }

    public void OnLeaderChangeBtn() {
        int selectedIndex = GetComponent<Index>().Id;
        int deckIndex = slots[selectedIndex].transform.GetChild(0).GetComponent<Index>().Id;
        Deck deck = accountManager.decks.Find(x => x.id == deckIndex);
        Modal.instantiate(deck.name + " 덱을 대표덱으로 지정하시겠습니까?", Modal.Type.YESNO, () => {
            accountManager.ChangeLeaderDeck(deckIndex);
        });
    }

    private void Clear() {
        foreach (GameObject slot in slots) {
            if (slot == null) return;
            foreach (Transform tf in slot.transform) {
                DestroyImmediate(tf.gameObject);
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

        //clone for preview tilegroup
        Transform tar = transform.Find("TileGroupParent");
        foreach(Transform tf in tar) DestroyImmediate(tf.gameObject);

        GameObject tg_clone = Instantiate(accountManager.transform.GetChild(0), tar).gameObject;
        tg_clone.transform.localScale = new Vector3(12, 12, 1);
        tg_clone.transform.localPosition = new Vector3(-270, 80, 1);
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