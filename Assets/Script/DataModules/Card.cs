using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataModules {
    [System.Serializable]
    public class Building : CardFromServerData { }

    [System.Serializable]
    public class Card {
        public string id;
        public string race;
        public string name;
        public string type;
        public string prodType;
        public int rareity;
        public string product;
        public int hitPoint;
        public int placementLimit;
        public bool canAttack;
        public Unit unit;
    }

    [System.Serializable]
    public class Unit {
        public int id;
        public string name;
        public string cost;
        public int coolTime;
        public int size;
        public int power;
        public float attackSpeed;
        public int detectRange;
        public int attackRange;
        public int hitPoint;
        public float moveSpeed;
        public int tearNeed;
    }

    public class CardFromServerData {
        public int id;
        public Card card;
    }
}