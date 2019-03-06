using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 최종적으로 배율을 적용하기 전까지 유저가 조작하는 정보를 저장하는 용도
/// </summary>
public class IngameUpgradeStream : MonoBehaviour {
    Dictionary<string, int> newRequests;        //새로운 할당 요청 <타입, 포인트>
    Dictionary<string, int> prevData;           //이전 할당 정보
    [SerializeField] PlayerController playerController;

    private void OnEnable() {
        Init();
    }

    public void Init() {
        newRequests = new Dictionary<string, int>();
        prevData = new Dictionary<string, int>();
    }

    public void Add(string key) {
        if (!newRequests.ContainsKey(key)) {
            newRequests.Add(key, 1);
        }
        else {
            newRequests[key] += 1;
        }

        //Debug.Log(key + " : " + newRequests[key]);
    }

    public void Remove(string key) {
        if (newRequests[key] == 1) newRequests.Remove(key);
        else newRequests[key] -= 1;
    }
}
