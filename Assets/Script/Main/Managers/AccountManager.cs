using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataModules;
using System.Linq;
using System;
using System.Text;
using Spine.Unity;


public partial class AccountManager : Singleton<AccountManager> {
    protected AccountManager() { }
    private NetworkManager _networkManager;
    MenuSceneEventHandler eventHandler;

    [Header(" - UserDeck")]
    public GameObject deckGroup;
    public GameObject defaultTileGroup;
    public int userTier = 0;
    public int selectNumber;
    public GameSceneManager.SceneState scenestate;

    

    //내 계정에서 사용가능한 카드뿐만 아니라 게임에 존재하는 모든 카드들의 정보
    public List<CardData> allCards;
    public List<NeutralMonsterData> neutralMonsterDatas;
    public class UserClassInput {
        public string nickname;
        public string deviceId;
    }

    [Serializable]
    public class UserClass {
        public int id;
        public string nickname;
        public string deviceId;
        public Card cards;
        public List<Deck> decks;
    }
    [Header(" - DeckList")]
    public List<Deck> decks = new List<Deck>();

    public int Exp { get; set; }
    public int Lv { get; set; }
    public string NickName { get; set; }
    public UserClass userInfos { get; set; }
    public int leaderIndex { get; set; }
    public int missionClear = 0;
    

    private Wallet wallet;

    private StringBuilder sb = new StringBuilder();
    //새로운 덱의 생성정보를 담는 임시 변수 (수정 예정)
    private Deck tmpData = null;
    private GameObject tmpObj;

    void Awake() {
        DontDestroyOnLoad(gameObject);
        _networkManager = NetworkManager.Instance;
        wallet = new Wallet();
        DEVICEID = SystemInfo.deviceUniqueIdentifier;
        eventHandler = MenuSceneEventHandler.Instance;

        GetAllCards();
        GetAllNeutralMonstersInfo();
    }

    public string DEVICEID { get; private set; }

    private void ReqUserInfoCallback(HttpResponse response) {
        //Server의 Wallet 정보 할당
        if (response.responseCode != 200) {
            if (response.responseCode == 400) {
                Debug.Log("저장 실패");
            }
            else {
                Debug.Log("알 수 없는 Server 오류");
                Debug.Log(response.responseCode);
            }
        }
        else {
            GetUserInfo();
            Debug.Log("저장 성공");
        }
    }

    public void GetAllCards() {
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/cards");
        _networkManager.request("GET", url.ToString(), GetAllCardsCallback, false);
    }

    private void GetAllCardsCallback(HttpResponse response) {
        if(response.responseCode == 200) {
            List<CardData> cards = JsonReader.Read<List<CardData>>(response.data);
            allCards = cards;
        }
    }

    public void GetAllNeutralMonstersInfo() {
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/info/creeps");
        _networkManager.request("GET", url.ToString(), GetAllNeutralMonstersInfoCallback, false);
    }

    private void GetAllNeutralMonstersInfoCallback(HttpResponse response) {
        if(response.responseCode == 200) {
            List<NeutralMonsterData> data = JsonReader.Read<List<NeutralMonsterData>>(response.data);
            neutralMonsterDatas = data;
        }
    }

    public CardData GetCardData(string id) {
        CardData card = allCards.Find(x => x.id == id);
        return card;
    }

    public CardData GetUnitCardData(string id) {
        CardData card = allCards.Find(x => x.unit != null && x.unit.id == id);
        return card;
    }

    public int GetGold() {
        return wallet.gold;
    }

    public int GetJewel() {
        return wallet.jewel;
    }

    public void GetUserInfo() {
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/" + DEVICEID);
        Debug.Log(url);
        _networkManager.request("GET", url.ToString(), OnUserReqCallback, false);
    }

    private void OnUserReqCallback(HttpResponse response) {
        if (response.responseCode == 200) {
            Modal.instantiate("로그인 되었습니다.", Modal.Type.CHECK, () => {
                userInfos = JsonReader.Read<UserClass>(response.data);
                LogoSceneController lgc = FindObjectOfType<LogoSceneController>();
                lgc.startButton();
                ConstructManager.Instance.SetAllBuildings();
            });
        }
        else if (response.responseCode == 404) {
            Debug.Log("저장되지 않은 계정");
            Modal.instantiate("새로운 계정을 등록합니다.", "닉네임을 입력하세요.", null, Modal.Type.INSERT, SetUserReqData);
        }
        else {
            Debug.Log(response.responseCode);
            Debug.Log("알 수 없는 Server 오류");
        }
    }

    private void SetUserReqData(string inputText) {
        UserClassInput userInfo = new UserClassInput();
        userInfo.nickname = inputText;
        userInfo.deviceId = DEVICEID;
        string json = JsonUtility.ToJson(userInfo);
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users");
        _networkManager.request("PUT", url.ToString(), json, ReqUserInfoCallback, false);

        ConstructManager.Instance.SetAllBuildings();
    }

    public enum Name {
        위니,
        미드레인지,
        OTK,
        빅
    }
}

/// <summary>
/// 플레이어 덱 관련 처리 (삭제, 수정, 추가)
/// </summary>
public partial class AccountManager {
    public void RemoveDeck(int id, GameObject obj) {
        Deck deck = decks.Find(x => x.id == id);
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/")
            .Append(DEVICEID)
            .Append("/decks/")
            .Append(id);
        tmpObj = obj;
        if (deck != null) {
            if (decks.Count == 1) {
                Modal.instantiate("덱이 하나밖에 없어 제거할 수 없습니다.", Modal.Type.CHECK);
            }
            else if (deck.isRepresent) {
                Modal.instantiate("대표 덱은 삭제할 수 없습니다.", Modal.Type.CHECK);
            }
            else {
                _networkManager.request("DELETE", url.ToString(), RemoveComplete);
            }
        }
    }

    private void RemoveComplete(HttpResponse response) {
        if (response.responseCode == 200) {
            if (tmpObj != null) {
                int slotNum = tmpObj.transform.parent.GetComponent<Index>().Id;
                Destroy(transform.GetChild(0).GetChild(slotNum).gameObject);
                Instantiate(defaultTileGroup, transform.GetChild(0)).SetActive(false);
            }
            eventHandler.PostNotification(MenuSceneEventHandler.EVENT_TYPE.INITIALIZE_DECK, this);
        }
    }

    public void AddDeck(Deck deck) {
        DeckPostForm form = new DeckPostForm();
        form.Name = deck.name;
        form.Race = "primal";
        form.IsRepresent = false;
        form.heroSerial = deck.heroSerial;
        form.activeSerial = deck.activeSerial;
        form.passiveSerial = deck.passiveSerial;
        if(deck.wildcardSerial != null) {
            form.wildcardSerial = deck.wildcardSerial;
        }
        else {
            form.wildcardSerial = new int[0];
        }
        var dataPack = JsonConvert.SerializeObject(form);

        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/")
            .Append(DEVICEID)
            .Append("/decks");
        Debug.Log(url);
        _networkManager.request("PUT", url.ToString(), dataPack.ToString(), AddDeckCallback);
    }

    public void ModifyDeck(Deck deck) {
        ModifyDeckPostForm form = new ModifyDeckPostForm();
        form.Name = deck.name;
        form.Race = "primal";
        form.IsRepresent = false;
        form.heroSerial = deck.heroSerial;
        form.activeSerial = deck.activeSerial;
        form.passiveSerial = deck.passiveSerial;
        if (deck.wildcardSerial != null) {
            form.wildcardSerial = deck.wildcardSerial;
        }
        else {
            form.wildcardSerial = new int[0];
        }
        var dataPack = JsonConvert.SerializeObject(form);

        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/")
            .Append(DEVICEID)
            .Append("/decks/")
            .Append(deck.id.ToString());
        Debug.Log(url);
        _networkManager.request("PUT", url.ToString(), dataPack.ToString(), ModifyDeckCallback);

        tmpData = deck;
    }

    public void ModifyDeckName(Deck deck, string newName) {
        if (deck == null) return;
        ModifyDeckPostForm form = new ModifyDeckPostForm();
        form.Name = deck.name;
        form.Race = "primal";
        form.IsRepresent = false;
        form.heroSerial = deck.heroSerial;
        form.activeSerial = deck.activeSerial;
        form.passiveSerial = deck.passiveSerial;
        if (deck.wildcardSerial != null) {
            form.wildcardSerial = deck.wildcardSerial;
        }
        else {
            form.wildcardSerial = new int[0];
        }
        var dataPack = JsonConvert.SerializeObject(form);

        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/")
            .Append(DEVICEID)
            .Append("/decks/")
            .Append(deck.id.ToString());
        _networkManager.request("PUT", url.ToString(), dataPack.ToString(), ModifyDeckCallback);

        tmpData = deck;
    }


    public void GetDeckDetail(int id) {
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/")
            .Append(DEVICEID)
            .Append("/decks/")
            .Append(id.ToString());
        _networkManager.request("GET", url.ToString(), GetDetailDeckCallback, false);
    }

    private void GetDetailDeckCallback(HttpResponse response) {
        if (response.responseCode == 200) {
            if (response.data != null) {
                DeckDetail deck = JsonReader.Read<DeckDetail>(response.data.ToString());
            }
        }
    }

    public void ChangeLeaderDeck(int id) {
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/")
            .Append(DEVICEID)
            .Append("/decks/change_represent/")
            .Append(id.ToString());
        WWWForm form = new WWWForm();
        Debug.Log(url.ToString());
        _networkManager.request("PUT", url.ToString(), "asd", ChangeLeaderDeckCallback);
    }

    private void ChangeLeaderDeckCallback(HttpResponse response) {
        if (response.responseCode == 200) {
            eventHandler.PostNotification(MenuSceneEventHandler.EVENT_TYPE.INITIALIZE_DECK, this);
        }
    }

    private void AddDeckCallback(HttpResponse response) {
        if (response.responseCode != 200) return;
        eventHandler.PostNotification(MenuSceneEventHandler.EVENT_TYPE.INITIALIZE_DECK, this);
    }

    private void ModifyDeckCallback(HttpResponse response) {
        if (response.responseCode != 200 || tmpData == null) return;
        Debug.Log(response.responseCode);

        Deck deck = decks.Find(x => x.id == tmpData.id);
        deck.name = tmpData.name;

        eventHandler.PostNotification(MenuSceneEventHandler.EVENT_TYPE.INITIALIZE_DECK, this);
    }



    public Deck FindDeck(int id) {
        Deck deck = decks.Find(x => x.id == id);
        return deck;
    }
}

/// <summary>
/// 플레이어 대표 덱 메인화면에 표시하기 위한 처리. 기타 건물 타일 관련 처리
/// </summary>
public partial class AccountManager {
    public void SetHQ(int num) {
        GameObject targetbuilding = FindObjectOfType<ConstructManager>().townCenter;
        GameObject targetTileGroup = transform.GetChild(0).GetChild(num).gameObject;
        int tileCount = targetTileGroup.transform.childCount - 1;

        if (targetTileGroup.transform.GetChild(tileCount / 2).childCount == 0) {
            if (targetbuilding != null) {
                GameObject targetTile = targetTileGroup.transform.GetChild(tileCount / 2).gameObject;
                targetbuilding = Instantiate(targetbuilding, targetTile.transform);
                targetTile.GetComponent<TileObject>().buildingSet = true;
                targetbuilding.transform.position = targetTile.transform.position;
                targetbuilding.GetComponent<BuildingObject>().setTileLocation = targetTile.GetComponent<TileObject>().tileNum;
                if(targetbuilding.GetComponent<SpriteRenderer>() != null)
                    targetbuilding.GetComponent<SpriteRenderer>().sortingOrder = tileCount * 2 - targetTile.GetComponent<TileObject>().tileNum;
                else
                    targetbuilding.GetComponent<MeshRenderer>().sortingOrder = tileCount * 2 - targetTile.GetComponent<TileObject>().tileNum;
            }
        }
    }
}

public partial class AccountManager {
    public MissionData mission;

    public void StartMission(int num) {
        if (num == 0 || num > 3) return;
        RequestMissionDeck(num);
    }


    public void RequestMissionDeck(int num) {
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/")
            .Append(DEVICEID)
            .Append("/mission/")
            .Append(num.ToString());
        WWWForm form = new WWWForm();
        Debug.Log(url.ToString());
        _networkManager.request("GET", url.ToString(), GetMissionData, false);
    }

    private void GetMissionData(HttpResponse response) {
        if (response.responseCode != 200) return;

        if (response.responseCode == 200) {
            if (response.data != null) {
                MissionData missionData = JsonReader.Read<MissionData>(response.data.ToString());
                mission = missionData;
                eventHandler.PostNotification(MenuSceneEventHandler.EVENT_TYPE.GET_MISSION_LOAD_COMPLETE, null, mission);
                MissionLoadComplete();
            }
        }
    }

    public void MissionLoadComplete() {
        GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
        if (mission != null) {
            Debug.Log(mission.title);
            gsm.startScene(scenestate, GameSceneManager.SceneState.IngameScene);
        }
    }

}