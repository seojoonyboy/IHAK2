using System.Collections.Generic;
using DataModules;

namespace DataModules {
    [System.Serializable]
    public class Deck {
        public int Id;
        public string Name;
        public Species.Type species;
        public bool isLeader;
        public List<BuildingTile> buildingTiles;
    }
}
