using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public partial class RespawnMinion : SerializedMonoBehaviour {
    
    public float time = 0f;
    public float respawnTime = 5f;

    private void Start() {
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
        //TODO : 베이스캠프 거점에서 미니언 늘리는 방식이 아닌 다른 방식으로 해야할 필요 있습니다.
        //if (unitGroup.MinionType() == "melee") minion = Instantiate(shortDisMinion, transform);
        //else if (unitGroup.MinionType() == "range") minion = Instantiate(longDisMinion, transform);
        //else 
        minion = null;
        return minion;
    }

    private void SetMinionInfo() {
        ActiveCard card = GetComponentInChildren<HeroAI>().unitCard;
        if(card == null) return;
        GameObject minion = SelectSpawnMinion();
        minion.GetComponent<MinionAI>().SetMinionData(card);
        minion.layer = transform.GetChild(2).gameObject.layer;
        //TODO : 베이스캠프 거점에서 미니언 늘리는 방식이 아닌 다른 방식으로 해야할 필요 있습니다.
        //minion.transform.position = unitGroup.transform.position;
        //unitGroup.ResetData();
    }
}

public partial class RespawnMinion : SerializedMonoBehaviour {
    [Header(" - Prefabs")]
    [SerializeField] GameObject shortDisMinion;
    [SerializeField] GameObject longDisMinion;
}