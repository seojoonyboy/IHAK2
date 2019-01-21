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
        public Cost product;
        public int hitPoint;
        public int placementLimit;
        public bool canAttack;
        public AttackInfo attackInfo;
        public Unit unit;
    }

    [System.Serializable]
    public class Unit {
        public int id;
        public string name;
        public Cost cost;
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

    [System.Serializable]
    public class Cost {
        public int id;
        public int food;
        public int gold;
        public int environment;
    }

    [System.Serializable]
    public class AttackInfo {
        public int id;
        public int power;
        public float attackSpeed;
        public float attackRange;
    }

    [System.Serializable]
    public class Skill {

    }

    public class CardFromServerData {
        public int id;
        public Card card;
    }
}