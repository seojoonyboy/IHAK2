using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataModules {
    [System.Serializable]
    public class BuildingInfo {
        public int tileNum;
        public bool activate;
        public CardData cardInfo;
        public GameObject gameObject;
        public BuildingInfo() { }

        public BuildingInfo(int tileNum, bool activate, CardData card, GameObject gameObject) {
            this.tileNum = tileNum;
            this.activate = activate;
            this.cardInfo = card;
            this.gameObject = gameObject;
        }
    }
}
