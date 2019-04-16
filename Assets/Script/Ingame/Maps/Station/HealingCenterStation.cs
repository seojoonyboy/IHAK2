using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingCenterStation : DefaultStation {

    [SerializeField]
    [ReadOnly] public List<GameObject> enemys;
    [ReadOnly] public List<GameObject> healingTarget;

    private bool startSeize = false;

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
            if (time == 100) {
                OwnerNum = (PlayerController.Player)enemys[0].gameObject.layer;
                GetComponent<Collider2D>().enabled = false;
                enemys.Clear();
                healingTarget.Clear();
                GetComponent<Collider2D>().enabled = true;
                startSeize = false;
            }
            if (healingTarget.Count > 0)
                startSeize = false;
            yield return new WaitForSeconds(0.1f);
            time++;
        }
    }

    void OnTriggerStay2D(Collider2D collision) {        
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!enemys.Exists(x => x == collision.gameObject)) enemys.Add(collision.gameObject);
        }
        if ((collision.gameObject.layer == (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!healingTarget.Exists(x => x == collision.gameObject)) enemys.Add(collision.gameObject);
            collision.gameObject.AddComponent<Heal>();
        }

        if (collision.name.CompareTo("Skeleton") != 0 && collision.gameObject.layer == (int)OwnerNum) {
            collision.gameObject.AddComponent<Heal>();
            collision.GetComponent<Heal>().delayTime = transform.GetChild(0).GetComponent<FieldHospital>().delayTime;
        }

        if (collision.gameObject.layer != (int)OwnerNum) {
            Heal heal = collision.gameObject.GetComponent<Heal>();
            if (heal != null) {
                Destroy(heal);
            }
        }

    }

    void OnTriggerExit2D(Collider2D collision) {
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            enemys.Remove(collision.gameObject);
        }
        if ((collision.gameObject.layer == (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!healingTarget.Exists(x => x == collision.gameObject)) enemys.Add(collision.gameObject);
            Heal heal = collision.gameObject.GetComponent<Heal>();
            if (heal != null)
                Destroy(heal);
        }

        if (collision.name.CompareTo("Skeleton") != 0) {
            Heal heal = collision.gameObject.GetComponent<Heal>();
            if (heal != null)
                Destroy(heal);
        }
    }
}
