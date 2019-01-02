using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using System.Linq;
using System;

public class PlayerInfosManager : Singleton<PlayerInfosManager> {
    protected PlayerInfosManager() { }
    public List<Deck> decks = new List<Deck>();

    [SerializeField]
    public List<List<int>> deckListData;
    [SerializeField]
    public List<int> selectDeck;








    void Awake() {
        DontDestroyOnLoad(gameObject);
        SetDummyDecks();
        
    }

    public void RemoveDeck(int id) {
        Deck deck = decks.Find(x => x.Id == id);
        if (deck.isLeader) {
            decks.Remove(deck);
            if(decks.Count > 0)
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
    }

    public void SetDummyDecks() {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int deckNum = 3;
        deckListData = new List<List<int>>();

        for (int i = 0; i < deckNum; i++) {
            Deck deck = new Deck();

            var species = (Species.Type[])Enum.GetValues(typeof(Species.Type));
            Species.Type selectedSpecies = species[UnityEngine.Random.Range(0, species.Length - 1)];
            var values = (Name[])Enum.GetValues(typeof(Name));
            Name selectedName = values[UnityEngine.Random.Range(0, values.Length - 1)];

            deck.Name = selectedName + " " + UnityEngine.Random.Range(0, 100).ToString();
            deck.species = selectedSpecies;
            deck.Id = i;
            deck.deckData = new List<int>();
            deck.deckData.Add(i);
            deckListData.Add(deck.deckData);
            

            if (i == 0) {
                deck.isLeader = true;
                selectDeck = deckListData[i];
            }
            
            decks.Add(deck);
        }
    }

    public enum Name {
        위니,
        미드레인지,
        OTK,
        빅
    }
}
