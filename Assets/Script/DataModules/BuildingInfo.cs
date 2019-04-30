using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataModules {
    [System.Serializable]
    public class BuildingInfo {
        public bool activate;
        public GameObject gameObject;

        public BuildingInfo(int tileNum, bool activate, CardData card, GameObject gameObject) {
            this.activate = activate;
            this.gameObject = gameObject;
        }
    }
}
