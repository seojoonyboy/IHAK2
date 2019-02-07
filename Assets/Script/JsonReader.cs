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
