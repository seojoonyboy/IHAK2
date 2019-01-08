using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class JsonReader : MonoBehaviour {

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public static List<T> Read<T>(string fileName, T _class) {
        TextAsset textAsset = Resources.Load(fileName) as TextAsset;
        Type type = _class.GetType();
        var result = JsonConvert.DeserializeObject<List<T>>(textAsset.text);
        return result;
    }
}
