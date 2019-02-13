using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataModules {
    [System.Serializable]
    public class Building {
        public int id;
        public Card card;
    }

    [System.Serializable]
    public class Card {
        public string id;
        public string race;
        public string name;
        public string type;
        public string prodType;
        public int rarity;
        public Cost product;
        public int hitPoint;
        public int placementLimit;
        public AttackInfo attackInfo;
        public Skill[] activeSkill;
        public Skill[] productSkills;
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
        public int detectRange;
        public int attackRange;
        public int hitPoint;
        public float moveSpeed;
        public int tearNeed;
        public string imageName;
    }

    [System.Serializable]
    public class Cost {
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
        public Cost cost;
        public int id;
        public string name;
        public SkillDetail method;
        public int tierNeed;
        public string imageName;
    }

    [System.Serializable]
    public class SkillDetail {
        public int id;
        public string methodName;
        public string args;
    }
}