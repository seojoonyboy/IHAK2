using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataModules {
    [System.Serializable]
    public class Card {
        public int id;
        public CardData data;
    }

    [System.Serializable]
    public class CardData {
        public string id;
        public string race;
        public string name;
        public string type;
        public int rarity;
        public Cost product;
        public int hitPoint;
        public int placementLimit;
        public Skill[] activeSkills;
        public Skill[] passiveSkills;
        public Unit unit;

        public int lv;
    }

    [System.Serializable]
    public class Unit {
        public string id;
        public string name;
        public Cost cost;
        public int coolTime;
        public int size;
        public int count;
        public int power;
        public float attackSpeed;
        public float defence;
        public int attackRange;
        public int hitPoint;
        public float moveSpeed;
        public decimal attackSP;
        public decimal beHitSP;
        public Skill skill;

        public string imageName;
    }

    [System.Serializable]
    public class Cost {
        public decimal gold;
        public decimal environment;
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
        public Cost cost;
        public int id;
        public string name;
        public SkillDetail method;
        public int tierNeed;
        public int coolTime;
        public string desc;
        public string imageName;
    }

    [System.Serializable]
    public class SkillDetail {
        public int id;
        public string methodName;
        public string args;
    }
}