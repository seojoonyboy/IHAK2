using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public GameSceneManager.SceneState scenestate;

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
    public bool userDataAvailable = false;
    public List<Deck> decks = new List<Deck>();

    public int Exp { get; set; }
    public int Lv { get; set; }
    public string NickName { get; set; }
    public UserClass userInfos { get; set; }
    public int leaderIndex { get; set; }


    private Wallet wallet;

    private StringBuilder sb = new StringBuilder();
    private int selId = -1;
    //새로운 덱의 생성정보를 담는 임시 변수 (수정 예정)
    private Deck tmpData = null;

    [SerializeField]
    public List<int> selectDeck;
    public int selectNumber;

    void Awake() {
        DontDestroyOnLoad(gameObject);
        _networkManager = NetworkManager.Instance;
        wallet = new Wallet();
        deviceID = SystemInfo.deviceUniqueIdentifier;
        //ReqUserInfo();
    }
    private void Start() {
        //deviceID = "11231234";
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
            GetUserInfo();
            Debug.Log("저장 성공");
        }
        else if (response.responseCode == 400) {
            Debug.Log("저장 실패");
        }
        else {
            Debug.Log("알 수 없는 Server 오류");
            Debug.Log(response.responseCode);
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
        selId = id;
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/")
            .Append(DEVICEID)
            .Append("/decks/")
            .Append(id);
        _networkManager.request("DELETE", url.ToString(), RemoveComplete);
    }

    private void RemoveComplete(HttpResponse response) {
        if (response.responseCode != 200 || selId == -1) return;
        Deck deck = decks.Find(x => x.id == selId);
        if (deck == null) return;
        decks.Remove(deck);
        MenuSceneEventHandler.Instance.PostNotification(MenuSceneEventHandler.EVENT_TYPE.DECKLIST_CHANGED, this);
    }

    public void AddDeck(Deck deck) {
        DeckPostForm form = new DeckPostForm();
        form.Name = deck.name;
        form.Race = "primal";
        form.IsRepresent = false;
        form.CoordsSerial = deck.coordsSerial;
        var dataPack = JsonConvert.SerializeObject(form);

        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/")
            .Append(DEVICEID)
            .Append("/decks");
        _networkManager.request("PUT", url.ToString(), dataPack.ToString(), AddDeckCallback);
    }

    public void ModifyDeck(Deck deck) {
        ModifyDeckPostForm form = new ModifyDeckPostForm();
        form.Id = deck.id;
        Debug.Log(deck.id);
        form.Name = deck.name;
        form.Race = "primal";
        form.IsRepresent = deck.isRepresent;
        form.CoordsSerial = deck.coordsSerial;
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

    private void AddDeckCallback(HttpResponse response) {
        if (response.responseCode != 200) return;
        Deck deck =  JsonConvert.DeserializeObject<Deck>(response.data);
        decks.Add(deck);
        
        MenuSceneEventHandler.Instance.PostNotification(MenuSceneEventHandler.EVENT_TYPE.DECKLIST_CHANGED, this);
    }

    private void ModifyDeckCallback(HttpResponse response) {
        if (response.responseCode != 200 || tmpData == null) return;

        Deck deck = decks.Find(x => x.id == tmpData.id);
        deck.name = tmpData.name;
        deck.coordsSerial = tmpData.coordsSerial;
        deck.coords = tmpData.coords;
        MenuSceneEventHandler.Instance.PostNotification(MenuSceneEventHandler.EVENT_TYPE.DECKLIST_CHANGED, this);
    }

    public Deck FindDeck(int id) {
        Deck deck = decks.Find(x => x.id == id);
        return deck;
    }

    public void ChangeLeaderDeck(int id, int index) {
        Deck deck = decks.Find(x => x.isRepresent ==  true);
        deck.isRepresent = false;
        //ModifyDeck(deck);
        deck = decks.Find(x => x.id == id);
        deck.isRepresent = true;
        ModifyDeck(deck);
        leaderIndex = index;
    }

    public void GetUserInfo() {
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/" + DEVICEID);
        _networkManager.request("GET", url.ToString(), OnUserReqCallback, false);
    }

    public void GetMyDecks() {
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users/deviceid/" + DEVICEID + "/decks");
        _networkManager.request("GET", url.ToString(), OnMyDeckReqCallback, false);
    }

    private void OnMyDeckReqCallback(HttpResponse response) {
        if (response.responseCode == 200) {
            if (userDataAvailable == false) {
                decks = JsonReader.Read(response.data.ToString(), new Deck());
                Deck deck = decks.FirstOrDefault();
                MenuSceneEventHandler.Instance.PostNotification(MenuSceneEventHandler.EVENT_TYPE.DECKLIST_CHANGED, this);
                userDataAvailable = true;
                SetTileObjects();
            }
            else {
                Debug.Log("저장된 데이터 활용");
                //checkDeck(selectNumber);
                MenuSceneEventHandler.Instance.PostNotification(MenuSceneEventHandler.EVENT_TYPE.DECKLIST_CHANGED, this);
            }
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
                userInfos = JsonConvert.DeserializeObject<UserClass>(response.data);
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
            Debug.Log("알 수 없는 Server 오류");
        }
    }
    

    /// <summary>
    /// 모든 건물 타일을 GameObject로 만드는 함수
    /// </summary>
    public void SetTileObjects() {
        if (decks == null)
            return;

        ConstructManager cm = ConstructManager.Instance;
        GameObject constructManager = cm.transform.gameObject;
        GameObject targetBuilding;


        for (int i = 0; i < decks.Count; i++) {
            for (int j = 0; j < transform.GetChild(0).GetChild(i).childCount; j++) {

                if (j == transform.GetChild(0).GetChild(i).childCount/2) {
                    targetBuilding = FindObjectOfType<ConstructManager>().townCenter;
                    if (targetBuilding != null && transform.GetChild(0).GetChild(i).GetChild(j).childCount == 0) {
                        GameObject setBuild = Instantiate(targetBuilding, transform.GetChild(0).GetChild(i).GetChild(j));
                        transform.GetChild(0).GetChild(i).GetChild(j).GetComponent<TileObject>().buildingSet = true;
                        setBuild.transform.position = transform.GetChild(0).GetChild(i).GetChild(j).position;
                        setBuild.GetComponent<BuildingObject>().setTileLocation = transform.GetChild(0).GetChild(i).GetChild(j).GetComponent<TileObject>().tileNum;
                        setBuild.GetComponent<SpriteRenderer>().sprite = setBuild.GetComponent<BuildingObject>().mainSprite;
                        setBuild.GetComponent<SpriteRenderer>().sortingOrder = setBuild.transform.parent.parent.childCount * 2 - setBuild.transform.parent.GetComponent<TileObject>().tileNum;
                        setBuild.AddComponent<LayoutGroup>();
                    }
                    continue;
                }
                

                targetBuilding = FindBuildingWithID(decks[i].coordsSerial[j]);
                if (targetBuilding != null) {
                    GameObject setBuild = Instantiate(targetBuilding, transform.GetChild(0).GetChild(i).GetChild(j));
                    transform.GetChild(0).GetChild(i).GetChild(j).GetComponent<TileObject>().buildingSet = true;
                    setBuild.transform.position = transform.GetChild(0).GetChild(i).GetChild(j).position;
                    setBuild.GetComponent<BuildingObject>().setTileLocation = transform.GetChild(0).GetChild(i).GetChild(j).GetComponent<TileObject>().tileNum;
                    setBuild.GetComponent<SpriteRenderer>().sprite = setBuild.GetComponent<BuildingObject>().mainSprite;
                    setBuild.GetComponent<SpriteRenderer>().sortingOrder = setBuild.transform.parent.parent.childCount*2 - setBuild.transform.parent.GetComponent<TileObject>().tileNum;
                    setBuild.AddComponent<LayoutGroup>();
                }
            }
        }
        //MenuSceneEventHandler.Instance.PostNotification(MenuSceneEventHandler.EVENT_TYPE.SET_TILE_OBJECTS_COMPLETED, this);
    }


    public void SetTileObjects(int num) {
        if (decks == null)
            return;

        ConstructManager cm = ConstructManager.Instance;
        GameObject constructManager = cm.transform.gameObject;
        GameObject targetBuilding;

        for (int i = 0; i < transform.GetChild(0).GetChild(num).childCount; i++) {
            if (i == transform.GetChild(0).GetChild(num).childCount / 2) {
                targetBuilding = FindObjectOfType<ConstructManager>().townCenter;
                if (targetBuilding != null && transform.GetChild(0).GetChild(num).GetChild(i).childCount == 0) {
                    GameObject setBuild = Instantiate(targetBuilding, transform.GetChild(0).GetChild(num).GetChild(i));
                    transform.GetChild(0).GetChild(num).GetChild(i).GetComponent<TileObject>().buildingSet = true;
                    setBuild.transform.position = transform.GetChild(0).GetChild(num).GetChild(i).position;
                    setBuild.GetComponent<BuildingObject>().setTileLocation = transform.GetChild(0).GetChild(num).GetChild(i).GetComponent<TileObject>().tileNum;
                    setBuild.GetComponent<SpriteRenderer>().sprite = setBuild.GetComponent<BuildingObject>().mainSprite;
                    setBuild.GetComponent<SpriteRenderer>().sortingOrder = setBuild.transform.parent.parent.childCount * 2 - setBuild.transform.parent.GetComponent<TileObject>().tileNum;
                    setBuild.AddComponent<LayoutGroup>();
                }
                continue;
            }

            targetBuilding = FindBuildingWithID(decks[num].coordsSerial[i]);
            if (targetBuilding != null) {
                GameObject setBuild = Instantiate(targetBuilding, transform.GetChild(0).GetChild(num).GetChild(i));
                transform.GetChild(0).GetChild(num).GetChild(i).GetComponent<TileObject>().buildingSet = true;
                setBuild.transform.position = transform.GetChild(0).GetChild(num).GetChild(i).position;
                setBuild.GetComponent<BuildingObject>().setTileLocation = transform.GetChild(0).GetChild(num).GetChild(i).GetComponent<TileObject>().tileNum;
                setBuild.GetComponent<SpriteRenderer>().sprite = setBuild.GetComponent<BuildingObject>().mainSprite;
                setBuild.GetComponent<SpriteRenderer>().sortingOrder = setBuild.transform.parent.parent.childCount * 2 - setBuild.transform.parent.GetComponent<TileObject>().tileNum;
                setBuild.AddComponent<LayoutGroup>();
            }
        }
        //MenuSceneEventHandler.Instance.PostNotification(MenuSceneEventHandler.EVENT_TYPE.SET_TILE_OBJECTS_COMPLETED, this);
    }

    public void checkDeck(int num) {
        if (decks == null)
            return;

        if (num > decks.Count - 1)
            return;

        ConstructManager cm = ConstructManager.Instance;
        GameObject constructManager = cm.transform.gameObject;
        GameObject targetBuilding;

        for (int i = 0; i < transform.GetChild(0).GetChild(num).childCount; i++) {
            if (i == transform.GetChild(0).GetChild(num).childCount / 2) {
                targetBuilding = FindObjectOfType<ConstructManager>().townCenter;
                if (targetBuilding != null && transform.GetChild(0).GetChild(num).GetChild(i).childCount == 0) {
                    GameObject setBuild = Instantiate(targetBuilding, transform.GetChild(0).GetChild(num).GetChild(i));
                    transform.GetChild(0).GetChild(num).GetChild(i).GetComponent<TileObject>().buildingSet = true;
                    setBuild.transform.position = transform.GetChild(0).GetChild(num).GetChild(i).position;
                    setBuild.GetComponent<BuildingObject>().setTileLocation = transform.GetChild(0).GetChild(num).GetChild(i).GetComponent<TileObject>().tileNum;
                    setBuild.GetComponent<SpriteRenderer>().sprite = setBuild.GetComponent<BuildingObject>().mainSprite;
                    setBuild.GetComponent<SpriteRenderer>().sortingOrder = setBuild.transform.parent.parent.childCount * 2 - setBuild.transform.parent.GetComponent<TileObject>().tileNum;
                    setBuild.AddComponent<LayoutGroup>();
                }
                continue;
            }


            if (decks[num].coordsSerial[i] != 0 && transform.GetChild(0).GetChild(num).GetChild(i).childCount == 0) {
                targetBuilding = FindBuildingWithID(decks[num].coordsSerial[i]);
                if (targetBuilding != null) {
                    GameObject setBuild = Instantiate(targetBuilding, transform.GetChild(0).GetChild(num).GetChild(i));
                    transform.GetChild(0).GetChild(num).GetChild(i).GetComponent<TileObject>().buildingSet = true;
                    setBuild.transform.position = transform.GetChild(0).GetChild(num).GetChild(i).position;
                    setBuild.GetComponent<BuildingObject>().setTileLocation = transform.GetChild(0).GetChild(num).GetChild(i).GetComponent<TileObject>().tileNum;
                    setBuild.GetComponent<SpriteRenderer>().sprite = setBuild.GetComponent<BuildingObject>().mainSprite;
                    setBuild.GetComponent<SpriteRenderer>().sortingOrder = setBuild.transform.parent.parent.childCount * 2 - setBuild.transform.parent.GetComponent<TileObject>().tileNum;
                    setBuild.AddComponent<LayoutGroup>();
                }
            }
            else if (decks[num].coordsSerial[i] == 0 && transform.GetChild(0).GetChild(num).GetChild(i).childCount != 0)
                Destroy(transform.GetChild(0).GetChild(num).GetChild(i).GetChild(0).gameObject);
            

            
            if (decks[num].coordsSerial[i] != 0) {
                transform.GetChild(0).GetChild(num).GetChild(i).GetComponent<TileObject>().buildingSet = true;
            }
            else if (decks[num].coordsSerial[i] <= 0)
                transform.GetChild(0).GetChild(num).GetChild(i).GetComponent<TileObject>().buildingSet = false;
                
        }


        

    }



    public void RemoveTileObjects(int num) {
        Transform targetTileGroup = gameObject.transform.GetChild(0).GetChild(num);
        foreach(Transform tile in targetTileGroup) {
            tile.GetComponent<TileObject>().buildingSet = false;
            foreach(Transform data in tile) {
                Destroy(data.gameObject);
            }
        }
        MenuSceneEventHandler.Instance.PostNotification(MenuSceneEventHandler.EVENT_TYPE.DECKLIST_CHANGED, null);
    }

    private void SetUserReqData(string inputText) {
        UserClassInput userInfo = new UserClassInput();
        userInfo.nickname = inputText;
        userInfo.deviceId = deviceID;
        string json = JsonUtility.ToJson(userInfo);
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl)
            .Append("api/users");
        _networkManager.request("PUT", url.ToString(), json, ReqUserInfoCallback, false);

        ConstructManager.Instance.SetAllBuildings();
    }

    
    
    public GameObject FindBuildingWithID(int ID) {

        GameObject buildingGroup = FindObjectOfType<ConstructManager>().transform.GetChild(0).gameObject;
        GameObject targetBuilding;       

        for (int i = 0; i < buildingGroup.transform.childCount; i++)  {
            if (buildingGroup.transform.GetChild(i).GetComponent<BuildingObject>().data.id == ID)  {
                targetBuilding = buildingGroup.transform.GetChild(i).gameObject;
                return targetBuilding;
             }
        }
        return null;        
    }

    public void SetHQ(int num) {

        GameObject targetbuilding = FindObjectOfType<ConstructManager>().townCenter;
        if (transform.GetChild(0).GetChild(num).GetChild(transform.GetChild(0).GetChild(num).childCount / 2).childCount == 0) {
            if(targetbuilding != null) {
                GameObject targetTile = transform.GetChild(0).GetChild(num).GetChild(transform.GetChild(0).GetChild(num).childCount / 2).gameObject;
                targetbuilding = Instantiate(targetbuilding, targetTile.transform);
                targetTile.GetComponent<TileObject>().buildingSet = true;
                targetbuilding.transform.position = targetTile.transform.position;
                targetbuilding.GetComponent<BuildingObject>().setTileLocation = targetTile.GetComponent<TileObject>().tileNum;
                targetbuilding.GetComponent<SpriteRenderer>().sortingOrder = targetTile.transform.parent.childCount * 2 - targetTile.GetComponent<TileObject>().tileNum;
            }            
        }
    }
    

    
    public enum Name {
        위니,
        미드레인지,
        OTK,
        빅
    }
}
