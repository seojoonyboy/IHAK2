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

public class Req_missionRead : JsonReader {

    [System.Serializable]
    public class MissionData : DataModules.MissionData {
        public new int stageNum;
        public new string title;
        public new DataModules.MonsterData[] creeps;
        public new Deck playerDeck;
        public new Deck opponentDeck;
        public new float hqHitPoint;
        public new DataModules.Conditions[] PlayerConditions;
        public new DataModules.Conditions[] opponentConditions;
    }

    [System.Serializable]
    public class Deck : DataModules.Deck  {
        public new int? id;
        public new string name;
        public new bool isRepresent;

        public new int[] heroSerial;
        public new int[] activeSerial;
        public new int[] passiveSerial;
        public new int[] wildcardSerial;

        public new Card[] cards;
    }

    [System.Serializable]
    public class Card : DataModules.Card {
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

        public int[] heroSerial;
        public int[] activeSerial;
        public int[] passiveSerial;
        public int[] wildcardSerial;

        public Card[] cards;
    }

    [System.Serializable]
    public class Card : DataModules.Card {
        public DataModules.CardData card { get { return data; } set { data = value; } }
    }
}