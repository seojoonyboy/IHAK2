using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StationBasic : MonoBehaviour {

    [SerializeField]
    protected StationState stationstate;

    // Use this for initialization
    void Start () {
        switch (stationstate) {
            case StationState.Creep:
                gameObject.AddComponent<CreepStation>();
                break;
            case StationState.Tower:
                gameObject.AddComponent<TowerStation>();                
                break;
            case StationState.Shop:
                gameObject.AddComponent<ShopStation>();
                break;
            case StationState.BaseCamp:
                gameObject.AddComponent<BaseCampStation>();
                break;
            case StationState.HealingCenter:
                gameObject.AddComponent<HealingCenterStation>();
                break;
            default:
                gameObject.AddComponent<PlayerBaseStation>();
                break;
        }
        StartCoroutine(DrawStationName());
	}

    IEnumerator DrawStationName() {
        yield return new WaitForSeconds(3.0f);
        GameObject name = Resources.Load("Prefabs/BuildingName") as GameObject;
        TextMeshPro stationName = Instantiate(name, transform).GetComponent<TextMeshPro>();
        stationName.text = stationstate.ToString();
    }

    public enum StationState {
        PlayerBase = 0,
        Creep = 1,
        Tower = 2,
        Shop= 3,
        BaseCamp = 4,
        HealingCenter = 5,
    }
}
