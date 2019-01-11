using System.Collections.Generic;
using DataModules;

namespace DataModules {
    [System.Serializable]
    public class Deck {
        public int id;
        public string name;
        public string race;
        public int[] coordsSerial;
        public int[,] coords;
    }
}
