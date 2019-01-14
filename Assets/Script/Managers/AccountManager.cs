using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using DataModules;
using System.Linq;
using System;
using System.Text;


public class AccountManager : Singleton<AccountManager> {
    protected AccountManager() { }
    private NetworkManager _networkManager;
    public GameObject deckGroup;
    private string deviceID;

    public List<Deck> decks = new List<Deck>();

    public int Exp { get; set; }
    public int Lv { get; set; }
    public string NickName { get; set; }

    private Wallet wallet;

    private StringBuilder sb = new StringBuilder();

    [SerializeField]
    public List<int> selectDeck;
    [HideInInspector]
    public int selectNumber;

    [Serializable]
    public class UserClass {
        public string Nickname;
        public string DeviceId;
    }

    void Awake() {
        DontDestroyOnLoad(gameObject);
        _networkManager = NetworkManager.Instance;
        wallet = new Wallet();
        //ReqUserInfo();
    }
    private void Start() {
        deviceID = SystemInfo.deviceUniqueIdentifier;
        if (deckGroup != null)
            Instantiate(deckGroup, transform);
    }

    public string DEVICEID {
        get { return deviceID; }
    }

    public void ReqUserInfo() {
        sb.Remove(0, sb.Length);
        _networkManager.request("GET", sb.ToString(), ReqUserInfoCallback);
    }

    private void ReqUserInfoCallback(HttpResponse response) {
        //Server의 Wallet 정보 할당
        if (response.responseCode == 200) {
            Debug.Log("저장 성공");
        }
        else if (response.responseCode == 400) {
            Debug.Log("저장 실패");
        }
        else {
            Debug.Log("알 수 없는 Server 오류");
        }
    }

    public int GetGold() {
        return wallet.gold;
    }

    public int GetJewel() {
        return wallet.jewel;
    }

    public void ChangeGoldAmnt(int amount = 0) {
        //sb.Remove(0, sb.Length);
        //sb.Append(_networkManager.baseUrl).Append(amount);
        //_networkManager.request("POST", sb.ToString(), OnChangeGold);
    }

    private void OnChangeGold(HttpResponse response) {
        //wallet.gold = 
        //EventHandler PostNotification 발생
    }

    public void RemoveDeck(int id) {
        Deck deck = decks.Find(x => x.id == id);
        decks.Remove(deck);
        //if (deck.isLeader) {
        //    decks.Remove(deck);
        //    if (decks.Count > 0)
        //        decks[0].isLeader = true;
        //}
        //else {
        //    decks.Remove(deck);
        //}
    }

    public void AddDeck(Deck deck) {
        if (decks.Count != 0) {
            int maxId = decks.Max(x => x.id);
            deck.id = maxId + 1;
        }
        else {
            deck.id = 0;
        }
        decks.Add(deck);
    }

    public Deck FindDeck(int id) {
        Deck deck = decks.Find(x => x.id == id);
        return deck;
    }

    public void ChangeLeaderDeck(int id) {
        //Deck prevLeaderDeck = decks.Find(x => x.isLeader == true);
        //if (prevLeaderDeck != null) prevLeaderDeck.isLeader = false;

        //Deck deck = decks.Find(x => x.id == id);
        //deck.isLeader = true;
        //selectDeck = deck.deckData;
    }

    public void GetUserInfo() {
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/" + deviceID);
        _networkManager.request("GET", url.ToString(), OnUserReqCallback, false);
    }

    public void GetMyDecks() {
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/6236213/decks");
        _networkManager.request("GET", url.ToString(), OnMyDeckReqCallback, false);
        
    }

    private void OnMyDeckReqCallback(HttpResponse response) {
        if (response.responseCode == 200) {
            decks = JsonReader.Read(response.data.ToString(), new Deck());
            Deck deck = decks.FirstOrDefault();
        }
        else if (response.responseCode == 404) {
            Debug.Log("페이지를 찾을 수 없습니다");
        }
        else {
            Debug.Log("알 수 없는 Server 오류");
        }        
    }

    private void OnUserReqCallback(HttpResponse response) {
        if (response.responseCode == 200) {
            Modal.instantiate("로그인 되었습니다.", Modal.Type.CHECK, () => {
                LogoSceneController lgc = FindObjectOfType<LogoSceneController>();
                lgc.startButton();
            });
        }
        else if (response.responseCode == 404) {
            Debug.Log("저장되지 않은 계정");

            Modal.instantiate("새로운 계정을 등록합니다.", Modal.Type.CHECK, () => {
                StringBuilder url = new StringBuilder();
                url.Append(_networkManager.baseUrl)
                    .Append("api/users");
                _networkManager.request("PUT", url.ToString(), SetUserjsonData(), ReqUserInfoCallback, false);
            });
        }
        else {
            Debug.Log("알 수 없는 Server 오류");
        }
    }


    public void SetTileObject() {
        if (decks == null)
            return;

        ConstructManager cm = ConstructManager.Instance;
        GameObject constructManager = cm.transform.gameObject;
        Debug.Log(constructManager.name);
        Debug.Log(decks.Count);


        for (int i = 0; i < decks.Count; i++) {            
            for(int j = 0; j < transform.GetChild(0).GetChild(i).childCount; j++) {
                GameObject setBuild = Instantiate(constructManager.transform.GetChild(0).GetChild(decks[i].coordsSerial[j]).gameObject, transform.GetChild(0).GetChild(i).GetChild(j));
                transform.GetChild(0).GetChild(i).GetChild(j).GetComponent<TileObject>().buildingSet = true;
                setBuild.transform.position = Vector2.zero;
            }
        }
    }

    private string SetUserjsonData() {
        UserClass userInfo = new UserClass();
        userInfo.Nickname = "TestUser001";
        userInfo.DeviceId = SystemInfo.deviceUniqueIdentifier;
        string json = JsonUtility.ToJson(userInfo);
        return json;
    }

    public enum Name {
        위니,
        미드레인지,
        OTK,
        빅
    }
}
