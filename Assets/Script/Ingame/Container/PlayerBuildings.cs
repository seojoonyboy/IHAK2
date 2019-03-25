using Container;
using DataModules;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBuildings : MonoBehaviour {
    [SerializeField] [ReadOnly] protected TileGroup tileGroup;
    [SerializeField] [ReadOnly] protected PlayerController playerController;

    public List<BuildingInfo> buildingInfos = new List<BuildingInfo>();

    public abstract void Init();
}
