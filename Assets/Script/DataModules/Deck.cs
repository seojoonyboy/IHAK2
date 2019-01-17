using System.Collections.Generic;
using DataModules;

namespace DataModules {
    [System.Serializable]
    public class Deck {
        public int id;
        public string name;
        public string race;
        public bool isRepresent;
        public int[] coordsSerial;
        public int[,] coords;
    }

    public class DeckPostForm {
        public string Name;
        public string Race;
        public bool IsRepresent;
        public int[] CoordsSerial;
    }

    public class ModifyDeckPostForm {
        public int Id;
        public string Name;
        public string Race;
        public bool IsRepresent;
        public int[] CoordsSerial;
    }
}
