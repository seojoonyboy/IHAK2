using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DataModules {
    [System.Serializable]
    public class MissionData {
        public int stageNum;
        public string title;
        public MonsterData[] creeps;
        public Deck playerDeck;
        public Deck opponentDeck;
        public float hqHitPoint;
    }

    [System.Serializable]
    public class MonsterData {
        public Monster creep;
        public int count;
    }

    [System.Serializable]
    public class Monster {
        public string id;
        public int rarity;
        public string name;
        public int size;
        public float attackPower;
        public float attackSpeed;
        public float defence;
        public float attackRange;
        public float hitPoint;
        public float moveSpeed;
        public int exp;
    }
}
