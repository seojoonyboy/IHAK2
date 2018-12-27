using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataModules {
    [System.Serializable]
    public class Deck {
        public int Id;
        public string Name;
        public Species.Type species;
        public bool isLeader;
        public GameObject settingDeck;
    }
}
