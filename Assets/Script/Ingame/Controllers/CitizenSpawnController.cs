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
            GameObject citizen = Instantiate(citizenPrefab, SpawnPos.GetChild(1));
            citizen.transform.position
                = new Vector3(CityPos.position.x + Random.Range(-5.0f, 5.0f), CityPos.position.y + Random.Range(-5.0f, 5.0f), CityPos.position.z);
            citizen.GetComponent<UnitAI>().enabled = false;
            citizens.Add(citizen);
        }
    }

    public void DeleteCitizen() {
        int realtimeCitizenNum = (int)PlayerController.Instance.playerResource().Citizen / 10;
        if (citizenNum > 0) {
            GameObject citizenObject = citizens[citizenNum-1];
            citizens.Remove(citizens[citizenNum-1]);
            Destroy(citizenObject);
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