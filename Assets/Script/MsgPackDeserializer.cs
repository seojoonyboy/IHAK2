using MsgPack;
using MsgPack.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MsgPackDeserializer : MonoBehaviour {

    // Use this for initialization
    void Start() {
        Data data = new Data();
        data.Name = "joonyboy";

        MemoryStream stream = new MemoryStream();
        MessagePackSerializer serializer = MessagePackSerializer.Get<Data>();
        serializer.Pack(stream, data);
        stream.Position = 0;

        var deserializedObject = (Data)serializer.Unpack(stream);

        Debug.Log(deserializedObject.Name);
    }

    // Update is called once per frame
    void Update() {

    }
}

public class Data {
    public string Name;
}
