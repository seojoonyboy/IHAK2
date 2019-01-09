using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using System.Linq;
using System;
using System.Text;

public class AccountManager : Singleton<AccountManager> {
    protected AccountManager() { }
    private NetworkManager _networkManager;

    public List<Deck> decks = new List<Deck>();

    public int Exp { get; set; }
    public int Lv { get; set; }
    public string NickName { get; set; }

    private Wallet wallet;

    private StringBuilder sb = new StringBuilder();

    [SerializeField]
    public List<int> selectDeck;

    void Awake() {
        DontDestroyOnLoad(gameObject);

        wallet = new Wallet();
        //ReqUserInfo();
    }

    public void ReqUserInfo() {
        sb.Remove(0, sb.Length);
        _networkManager.request("GET", sb.ToString(), ReqUserInfoCallback);
    }

    private void ReqUserInfoCallback(HttpResponse response) {
        //Server의 Wallet 정보 할당
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
        Deck deck = decks.Find(x => x.Id == id);
        if (deck.isLeader) {
            decks.Remove(deck);
            if (decks.Count > 0)
                decks[0].isLeader = true;
        }
        else {
            decks.Remove(deck);
        }
    }

    public void AddDeck(Deck deck) {
        if (decks.Count != 0) {
            int maxId = decks.Max(x => x.Id);
            deck.Id = maxId + 1;
        }
        else {
            deck.Id = 0;
        }
        decks.Add(deck);
    }

    public Deck FindDeck(int id) {
        Deck deck = decks.Find(x => x.Id == id);
        return deck;
    }

    public void ChangeLeaderDeck(int id) {
        Deck prevLeaderDeck = decks.Find(x => x.isLeader == true);
        if (prevLeaderDeck != null) prevLeaderDeck.isLeader = false;

        Deck deck = decks.Find(x => x.Id == id);
        deck.isLeader = true;
        //selectDeck = deck.deckData;
    }

    public void SetDummyDecks(ref Dictionary<Building.Category, List<GameObject>> buildings) {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int deckNum = 3;

        for (int i = 0; i < deckNum; i++) {
            Deck deck = new Deck();

            var species = (Species.Type[])Enum.GetValues(typeof(Species.Type));
            Species.Type selectedSpecies = species[UnityEngine.Random.Range(0, species.Length - 1)];
            var values = (Name[])Enum.GetValues(typeof(Name));
            Name selectedName = values[UnityEngine.Random.Range(0, values.Length - 1)];

            deck.Name = selectedName + " " + UnityEngine.Random.Range(0, 100).ToString();
            deck.species = selectedSpecies;
            deck.Id = i;

            deck.buildingTiles = new List<BuildingTile>();

            var buildingTypes = (Building.Category[])Enum.GetValues(typeof(Building.Category));
            foreach(Building.Category category in buildingTypes) {
                var lists = buildings[category];
                int[] rndArray = RndNumGenerator.getRandomInt(3, 0, lists.Count - 1);
                Coord[] coords = new Coord[] { new Coord(0, 0), new Coord(0, 1), new Coord(0, 2) };
                int coordIndex = 0;
                foreach (int num in rndArray) {
                    BuildingTile bt = new BuildingTile();
                    bt.data = lists[num].GetComponent<BuildingObject>().data;
                    bt.coord = coords[coordIndex];
                    deck.buildingTiles.Add(bt);
                    coordIndex++;
                }
            }

            if (i == 0) {
                deck.isLeader = true;
            }
            decks.Add(deck);
        }
        Debug.Log("!!");
    }

    public enum Name {
        위니,
        미드레인지,
        OTK,
        빅
    }
}
