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
        public string name;
        public string type;
        public int rarity;
        public ActiveSkill[] activeSkills;
        public PassiveSkill[] passiveSkills;
        public Unit unit;
    }

    [System.Serializable]
    public class Unit {
        public string id;
        public string name;
        public int rarity;
        public Cost cost;
        public int coolTime;
        public int size;
        public int attackPower;
        public float attackSpeed;
        public float defence;
        public float attackRange;
        public int hitPoint;
        public float moveSpeed;
        public UnitSkill skill;
        public Minion minion;

        public string imageName;
    }

    [System.Serializable]
    public class Cost {
        public decimal gold;
        public decimal population;
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
        public int id;
        public string name;
        public SkillDetail method;
        public string desc;
    }

    [System.Serializable]
    public class ActiveSkill : Skill {
        public Cost cost;
        public int coolTime;
    }

    [System.Serializable]
    public class UnitSkill : Skill {
        public Cost cost;
        public int coolTime;
    }

    [System.Serializable]
    public class PassiveSkill : Skill { }

    [System.Serializable]
    public class SkillDetail {
        public int id;
        public string methodName;
        public string[] args;
    }

    [System.Serializable]
    public class UnitSkillDetail {
        public string methodName;
        public string args;
    }

    [System.Serializable]
    public class Minion {
        public int id;
        public string type;
        public int count;
        public int[] capabilityArgs;
    }
}