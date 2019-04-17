using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class TowerStation : DefaultStation {
    [SerializeField] [ReadOnly] protected bool startSeize = false;
    
    // Use this for initialization
    void Start () {
        OwnerNum = PlayerController.Player.NEUTRAL;
        StationIdentity = StationBasic.StationState.Tower;
        targets = new List<GameObject>();
        Building = Resources.Load("Prefabs/Tower") as GameObject;
        GameObject tower = Instantiate(Building, transform);
        towerComponent = tower.GetComponent<Tower_Detactor>();
    }
	
	// Update is called once per frame
	void Update () {
        if (towerComponent.IsDestroyed && !startSeize) {
            startSeize = true;
            StartCoroutine(FindOwner());
        }
	}

    IEnumerator FindOwner() {
        int targetLayer = 0;
        while (startSeize) {
            foreach (GameObject target in targets) {
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
            OwnerNum = (PlayerController.Player)targetLayer;
            GetComponent<Collider2D>().enabled = false;
            targets.Clear();
            GetComponent<Collider2D>().enabled = true;
            StartCoroutine(RebuildTower());
            startSeize = false;
        }
    }

    IEnumerator RebuildTower() {
        yield return new WaitForSeconds(8.0f);
        Building = Resources.Load("Prefabs/Tower") as GameObject;
        GameObject tower = Instantiate(Building, transform);
        towerComponent = tower.GetComponent<Tower_Detactor>();
    }
}

public partial class TowerStation : DefaultStation {
    [SerializeField] [ReadOnly] public List<GameObject> targets;
    public Tower_Detactor towerComponent;

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == 16) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 16) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            targets.Remove(collision.gameObject);
        }
    }
}
