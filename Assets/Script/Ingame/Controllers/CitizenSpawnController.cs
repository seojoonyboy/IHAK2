using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using Sirenix.OdinInspector;

public partial class CitizenSpawnController : SerializedMonoBehaviour {

    List<GameObject> citizens;
    int citizenNum;

    // Use this for initialization
    void Start () {
        citizens = new List<GameObject>();
        citizenNum = 0;
        SpawnPos.transform.position = CityPos.position;
    }

    public void AddCitizen() {
        int realtimeCitizenNum = (int)PlayerController.Instance.playerResource().Citizen / 10;
        if (realtimeCitizenNum > citizenNum) {
            citizenNum++;
            GameObject citizen = Instantiate(citizenPrefab, SpawnPos.GetChild(Random.Range(0, 4)).transform);
            citizen.GetComponent<UnitAI>().enabled = false;
            citizens.Add(citizen);
        }
    }

    public void DeleteCitizen() {
        int realtimeCitizenNum = (int)PlayerController.Instance.playerResource().Citizen / 10;
        if (citizenNum > 0) {
            Destroy(citizens[citizenNum]);
            citizenNum--;
            PlayerController.Instance.playerResource().Citizen -= 10;
        }
    }
}

public partial class CitizenSpawnController : SerializedMonoBehaviour {
    [Header(" - Player Identity")]
    [SerializeField] PlayerController.Player playerNum;
    [SerializeField] Transform SpawnPos;
    [SerializeField] Transform CityPos;

    [Header(" - Prefabs")]
    [SerializeField] GameObject citizenPrefab;
}