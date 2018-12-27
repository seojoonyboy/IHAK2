using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using System.Linq;
using System;

public class PlayerInfosManager : Singleton<PlayerInfosManager> {
    protected PlayerInfosManager() { }
    public List<Deck> decks = new List<Deck>();

    void Awake() {
        DontDestroyOnLoad(gameObject);
        SetDummyDecks();
    }

    public void RemoveDeck(int id) {
        Deck deck = decks.Find(x => x.Id == id);
        decks.Remove(deck);
    }

    public void AddDeck(Deck deck) {
        if(decks.Count != 0) {
            int maxId = decks.Max(x => x.Id);
            deck.Id = maxId + 1;
        }
        else {
            deck.Id = 0;
        }
        decks.Add(deck);
    }

    public void ChangeLeaderDeck(int id) {
        Deck prevLeaderDeck = decks.Find(x => x.isLeader == true);
        prevLeaderDeck.isLeader = false;

        Deck deck = decks.Find(x => x.Id == id);
        deck.isLeader = true;
    }

    public void SetDummyDecks() {
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

            if (i == 0) {
                deck.isLeader = true;
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
