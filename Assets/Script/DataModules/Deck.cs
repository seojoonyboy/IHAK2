using System.Collections.Generic;
using DataModules;

namespace DataModules {
    [System.Serializable]
    public class Deck {
        public int id;
        public string name;
        public bool isRepresent;

        public int[] heroSerial;
        public int[] activeSerial;
        public int[] passiveSerial;
        public int[] wildcardSerial;

        public Card[] cards;
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

    [System.Serializable]
    public class DeckDetail : Deck { }

    [System.Serializable]
    public class ProductResources {
        public Resource food;
        public Resource gold;
        public Resource env;
        public Resource all;
    }

    [System.Serializable]
    public class Resource {
        public int id;
        public int food;
        public int gold;
        public int environment;
    }
}
