using Container;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayerController : MonoBehaviour {
    public List<BuildingInfo> buildingInfos;

    void Start() {
        buildingInfos = new List<BuildingInfo>();
        GetComponent<IngameEnemyGenerator>().Generate();
    }
}
