using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameUpgradeStream : MonoBehaviour {
    Dictionary<string, int> newRequests;        //새로운 할당 요청
    Dictionary<string, int> prevData;           //이전 할당 정보
    [SerializeField] PlayerController playerController;

    private void OnEnable() {
        Init();
    }

    public void Init() {
        newRequests = new Dictionary<string, int>();
        prevData = new Dictionary<string, int>();
    }

    public void Add() {

    }

    public void Remove() {

    }
}
