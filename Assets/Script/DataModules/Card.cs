using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataModules {
    [System.Serializable]
    public class Building : CardFromServerData { }
    [System.Serializable]
    public class Unit : CardFromServerData { }

    [System.Serializable]
    public class Card {
        public int id;
        public string race;
        public string name;
        public string type;
        public int rareity;
        public int hitPoint;
        public int placementLimit;
        public bool canAttack;
    }

    public class CardFromServerData {
        public int id;
        public Card card;
    }
}