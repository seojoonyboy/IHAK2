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
        PlayerController.Instance.transform.GetComponent<Container.PlayerResource>().UseCitizen(1);

        minion.GetComponent<MinionAI>().SetUnitData(null, null);
        minion.layer = gameObject.layer;
        minion.transform.position = unitGroup.transform.position;
        unitGroup.ResetData();
    }
}

public partial class RespawnMinion : SerializedMonoBehaviour {
    [Header(" - Prefabs")]
    [SerializeField] GameObject shortDisMinion;
    [SerializeField] GameObject longDisMinion;
}