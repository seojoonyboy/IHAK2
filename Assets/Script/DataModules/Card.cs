using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataModules {
    [System.Serializable]
    public class Building : Card {
        public enum Category {
            PRODUCT,
            MILITARY,
            SPECIAL
        }
    }
    [System.Serializable]
    public class Unit : Card { }
    [System.Serializable]
    public class Card {
        public int Id;
        public string Name;
        public int Tier;

        public int Hp;

        public int Limit_num;

        public string Type;
        public string Primal;
    }
}