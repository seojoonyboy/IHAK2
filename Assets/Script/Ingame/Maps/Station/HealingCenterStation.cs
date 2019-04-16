using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingCenterStation : DefaultStation {

    [SerializeField]
    [ReadOnly] public List<GameObject> targets;

    // Use this for initialization
    void Start () {
        OwnerNum = PlayerController.Player.NEUTRAL;
        StationIdentity = StationBasic.StationState.HealingCenter;
        targets = new List<GameObject>();
        Building = Resources.Load("Prefabs/FieldHospital") as GameObject;
        Instantiate(Building, transform);
    }
	

    IEnumerator FindOwner() {
        bool find = true;
        int targetLayer = 0;
        while (find) {
            foreach (GameObject target in targets) {
                if (target == null) continue;
                if ((int)OwnerNum != target.layer) {
                    int tempLayer = targetLayer;
                    targetLayer = target.layer;
                    if (tempLayer != targetLayer)
                        find = false;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
        OwnerNum = (PlayerController.Player)targetLayer;
    }

    void OnTriggerStay2D(Collider2D collision) {        
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
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
            targets.Remove(collision.gameObject);
        }

        if (collision.name.CompareTo("Skeleton") != 0) {
            Heal heal = collision.gameObject.GetComponent<Heal>();
            if (heal != null)
                Destroy(heal);
        }
    }
}
