using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEventHandler : MonoBehaviour {

    // Use this for initialization
    void Start() {
        UnitEventHandler.Instance.AddListener(UnitEventHandler.EVENT_TYPE.GENERATED, OnGenerate);
        //Invoke("Test", 1.0f);
        UnitEventHandler.Instance.PostNotification(UnitEventHandler.EVENT_TYPE.GENERATED, this);
    }

    void Test() {
        UnitEventHandler.Instance.PostNotification(UnitEventHandler.EVENT_TYPE.GENERATED, this);
    }

    private void OnGenerate(Enum Event_Type, Component Sender, object Param) {
        Debug.Log("유닛 생성");
    }

    // Update is called once per frame
    void Update() {

    }
}
