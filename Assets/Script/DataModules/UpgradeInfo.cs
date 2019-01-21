using UnityEngine;

namespace DataModules {
    [System.Serializable]
    public class UpgradeInfo : ScriptableObject {
        public int rareity;
        public int maintainence;
        public int hp;
        public Resources product;
        public Resources upgradeCost;
    }

    [System.Serializable]
    public class Resources {
        public int gold;
        public int food;
        public int env;
    }
}