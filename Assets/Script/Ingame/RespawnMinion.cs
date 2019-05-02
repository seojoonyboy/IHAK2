using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public partial class RespawnMinion : SerializedMonoBehaviour {
    
    public float time = 0f;
    public float respawnTime = 5f;
    [ReadOnly] public UnitGroup unitGroup;

    private void Start() {
        unitGroup = transform.GetComponent<UnitGroup>();
        shortDisMinion = PlayerController.Instance.transform.GetComponent<MinionSpawnController>().ShortDisMinion;
        longDisMinion = PlayerController.Instance.transform.GetComponent<MinionSpawnController>().LongDisMinion;
    }

    private void Update() {
        time += Time.deltaTime;
        if(time >= respawnTime) {
            SpawnMinion();
            time = 0;
        }
    }

    private void SpawnMinion() {
        if (unitGroup.IsMinionMax() == true) return;
        GameObject minion;

        if (unitGroup.MinionType() == "melee") minion = Instantiate(shortDisMinion, transform);
        else if (unitGroup.MinionType() == "range") minion = Instantiate(longDisMinion, transform);
        else minion = null;
        if (minion == null) return;

        if (transform.GetChild(2).gameObject.layer == 11) {
            EnemyPlayerController epc = GameObject.Find("EnemyPlayerController").GetComponent<EnemyPlayerController>();
            if(epc.AiCitizen > 1) {
                ActiveCard card = GetComponentInChildren<HeroAI>().unitCard;
                minion.GetComponent<MinionAI>().SetMinionData(card);
                minion.layer = transform.GetChild(2).gameObject.layer;
                minion.transform.position = unitGroup.transform.position;
                unitGroup.ResetData();
                minion.GetComponent<UnitAI>().enabled = false;
            }
            return;
        }

        if (PlayerController.Instance.playerResource().citizen_readonly >= 100) {
            PlayerController.Instance.transform.GetComponent<Container.PlayerResource>().UseCitizen(1);
            ActiveCard card = GetComponentInChildren<HeroAI>().unitCard;
            minion.GetComponent<MinionAI>().SetMinionData(card);
            minion.layer = transform.GetChild(2).gameObject.layer;
            minion.transform.position = unitGroup.transform.position;
            unitGroup.ResetData();
            minion.GetComponent<UnitAI>().enabled = false;
        }
        else
            return;
    }
}

public partial class RespawnMinion : SerializedMonoBehaviour {
    [Header(" - Prefabs")]
    [SerializeField] GameObject shortDisMinion;
    [SerializeField] GameObject longDisMinion;
}