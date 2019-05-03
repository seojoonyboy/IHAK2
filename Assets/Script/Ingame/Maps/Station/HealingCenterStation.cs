using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class HealingCenterStation : DefaultStation {

    [SerializeField] [ReadOnly] protected bool startSeize = false;
    [SerializeField] [ReadOnly] protected int seizePlayer;

    // Use this for initialization
    void Start () {
        OwnerNum = PlayerController.Player.NEUTRAL;
        StationIdentity = StationBasic.StationState.HealingCenter;
        enemys = new List<GameObject>();
        healingTarget = new List<GameObject>();
        Building = Resources.Load("Prefabs/FieldHospital") as GameObject;
        Instantiate(Building, transform);
        LoadFogLight();
    }

    private void Update() {
        //if(OwnerNum == PlayerController.Player.NEUTRAL && !startSeize && enemys.Count > 0) {
        //    startSeize = true;
        //    seizePlayer = enemys[0].layer;
        //    StartCoroutine(SeizeNeutralBuilding());
        //}
        if (!startSeize && enemys.Count > 0 && healingTarget.Count == 0) {
            startSeize = true;
            StartCoroutine(SeizeBuilding());
        }
    }

    public override void DestroyEnteredTarget(GameObject unitObj) {
        if (enemys.Contains(unitObj)) enemys.Remove(unitObj);
        if (healingTarget.Contains(unitObj)) healingTarget.Remove(unitObj);
    }

    IEnumerator SeizeBuilding() {
        int time = 0;
        while (startSeize) {
            if (enemys.Count == 0) {
                startSeize = false;
                break;
            }
            if (time == 100) {
                OwnerNum = (PlayerController.Player)enemys[0].gameObject.layer;
                GetComponent<Collider2D>().enabled = false;
                enemys.Clear();
                healingTarget.Clear();
                GetComponent<Collider2D>().enabled = true;
                startSeize = false;
                if (OwnerNum == PlayerController.Player.PLAYER_1)
                    IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.MISSION_EVENT.NODE_CAPTURE_COMPLETE, this, null);
            }
            if (healingTarget.Count > 0) startSeize = false;
            yield return new WaitForSeconds(0.1f);
            time++;
        }

    }

    IEnumerator SeizeNeutralBuilding() {
        int targetLayer = 0;
        while (startSeize) {
            foreach (GameObject target in enemys.ToList()) {
                if (target == null) continue;
                if (targetLayer == 0) {
                    targetLayer = target.layer;
                    continue;
                }
                if ((int)OwnerNum != target.layer) {
                    yield return new WaitForSeconds(0.1f);
                    if (targetLayer != target.layer) startSeize = false;
                }
            }
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(SeizeBuilding());
            startSeize = false;
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
            if(OwnerNum == PlayerController.Player.NEUTRAL) {
                if (seizePlayer != collision.gameObject.layer)
                    startSeize = false;
            }
        }
        if ((collision.gameObject.layer == (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!healingTarget.Exists(x => x == collision.gameObject)) healingTarget.Add(collision.gameObject);
            collision.gameObject.AddComponent<Heal>();
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 16) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            enemys.Remove(collision.gameObject);
        }
        if ((collision.gameObject.layer == (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (healingTarget.Exists(x => x == collision.gameObject)) healingTarget.Remove(collision.gameObject);
            Heal heal = collision.gameObject.GetComponent<Heal>();
            if (heal != null)
                Destroy(heal);
        }
    }
}

