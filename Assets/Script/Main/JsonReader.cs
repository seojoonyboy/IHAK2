using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class JsonReader {
    public static T Read<T>(string data) {
        return (T)Convert.ChangeType(JsonConvert.DeserializeObject<T>(data), typeof(T));
    }

    //public static T Read<T>(string data, T _class) {
    //    Type type = _class.GetType();
    //    var result = JsonConvert.DeserializeObject<List<T>>(data);
    //    return (T)Convert.ChangeType(result, typeof(T));
    //}
}

public class Req_cardsInventoryRead : JsonReader {
    [System.Serializable]
    public class Card : DataModules.Card{
        public DataModules.CardData card { get { return data; } set { data = value; } }
    }
}

public class Req_deckDetail : JsonReader {
    [System.Serializable]
    public class Deck {
        public int id;
        public string name;
        public string race;
        public bool isRepresent;
        public int[] coordsSerial;
        public int[,] coords;
        public Card[] cards;
    }

    [System.Serializable]
    public class Card : DataModules.Card {
        public DataModules.CardData card { get { return data; } set { data = value; } }
    }
}