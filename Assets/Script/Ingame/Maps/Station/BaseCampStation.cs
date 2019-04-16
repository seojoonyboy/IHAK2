using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BaseCampStation : DefaultStation {

    List<GameObject> creepList;

    [SerializeField]
    public List<GameObject> targets;

    // Use this for initialization
    void Start () {
        OwnerNum = PlayerController.Player.NEUTRAL;
        creepList = new List<GameObject>();
        targets = new List<GameObject>();
        Building = Resources.Load("Prefabs/FowardHQ") as GameObject;
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
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null && targets.Contains(collision.gameObject) == false) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            targets.Remove(collision.gameObject);
        }
    }


    // Update is called once per frame
    void Update () {
		if(creepList.Count == 0) {

        }
	}
}
