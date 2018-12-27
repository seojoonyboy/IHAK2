using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;

public class PlayerInfosManager : Singleton<PlayerInfosManager> {
    protected PlayerInfosManager() { }
    public Deck[] decks;
    void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
