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

        if (transform.GetChild(2).gameObject.layer == 11) {
            EnemyPlayerController epc = FindObjectOfType<EnemyPlayerController>();
            if(epc.AiCitizen > 1) SetMinionInfo();
        }
        else if (PlayerController.Instance.playerResource().citizen_readonly >= 100) {
            PlayerController.Instance.transform.GetComponent<Container.PlayerResource>().UseCitizen(1);
            SetMinionInfo();
        }
    }

    private GameObject SelectSpawnMinion() {
        GameObject minion;
        if (unitGroup.MinionType() == "melee") minion = Instantiate(shortDisMinion, transform);
        else if (unitGroup.MinionType() == "range") minion = Instantiate(longDisMinion, transform);
        else minion = null;
        return minion;
    }

    private void SetMinionInfo() {
        ActiveCard card = GetComponentInChildren<HeroAI>().unitCard;
        if(card == null) return;
        GameObject minion = SelectSpawnMinion();
        minion.GetComponent<MinionAI>().SetMinionData(card);
        minion.layer = transform.GetChild(2).gameObject.layer;
        minion.transform.position = unitGroup.transform.position;
        unitGroup.ResetData();
    }
}

public partial class RespawnMinion : SerializedMonoBehaviour {
    [Header(" - Prefabs")]
    [SerializeField] GameObject shortDisMinion;
    [SerializeField] GameObject longDisMinion;
}