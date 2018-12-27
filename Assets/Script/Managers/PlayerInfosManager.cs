using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;

public class PlayerInfosManager : Singleton<PlayerInfosManager> {
    protected PlayerInfosManager() { }
    public List<Deck> decks = new List<Deck>();
    void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void RemoveDeck(int id) {
        Deck deck = decks.Find(x => x.Id == id);
        decks.Remove(deck);
    }
}
