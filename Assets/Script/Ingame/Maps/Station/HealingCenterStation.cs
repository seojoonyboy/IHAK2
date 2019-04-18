using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class HealingCenterStation : DefaultStation {

    [SerializeField] [ReadOnly] protected bool startSeize = false;

    // Use this for initialization
    void Start () {
        OwnerNum = PlayerController.Player.NEUTRAL;
        StationIdentity = StationBasic.StationState.HealingCenter;
        enemys = new List<GameObject>();
        healingTarget = new List<GameObject>();
        Building = Resources.Load("Prefabs/FieldHospital") as GameObject;
        Instantiate(Building, transform);
    }

    private void Update() {
        if(!startSeize && enemys.Count > 0 && healingTarget.Count == 0) {
            startSeize = true;
            StartCoroutine(SeizeBuilding());
        }
    }

    IEnumerator SeizeBuilding() {
        int time = 0;
        while (startSeize) {
            if (enemys.Count == 0) startSeize = false;
            if (time == 200) {
                OwnerNum = (PlayerController.Player)enemys[0].gameObject.layer;
                GetComponent<Collider2D>().enabled = false;
                enemys.Clear();
                healingTarget.Clear();
                GetComponent<Collider2D>().enabled = true;
                startSeize = false;
            }
            if (healingTarget.Count > 0) startSeize = false;
            yield return new WaitForSeconds(0.1f);
            time++;
        }
    }

    
}

public partial class HealingCenterStation : DefaultStation {

    [SerializeField]
    [ReadOnly] public List<GameObject> enemys;
    [ReadOnly] public List<GameObject> healingTarget;

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == 16) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!enemys.Exists(x => x == collision.gameObject)) enemys.Add(collision.gameObject);
        }
        if ((collision.gameObject.layer == (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!healingTarget.Exists(x => x == collision.gameObject)) enemys.Add(collision.gameObject);
            collision.gameObject.AddComponent<Heal>();
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 16) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            enemys.Remove(collision.gameObject);
        }
        if ((collision.gameObject.layer == (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!healingTarget.Exists(x => x == collision.gameObject)) enemys.Add(collision.gameObject);
            Heal heal = collision.gameObject.GetComponent<Heal>();
            if (heal != null)
                Destroy(heal);
        }
    }
}

