using ingameUIModules;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class TowerStation : DefaultStation {
    [SerializeField] [ReadOnly] protected bool startSeize = false;
    [SerializeField] [ReadOnly] protected bool rebuilding = false;
    [SerializeField] [ReadOnly] GameObject occupySlider;
    [SerializeField] [ReadOnly] CircularSlider circularSlider;

    IEnumerator coroutine;
    bool isAlreadyCoroutine = false;
    // Use this for initialization
    void Start () {
        OwnerNum = PlayerController.Player.NEUTRAL;
        StationIdentity = StationBasic.StationState.Tower;
        targets = new List<GameObject>();
        Building = Resources.Load("Prefabs/Tower") as GameObject;
        GameObject tower = Instantiate(Building, transform);
        tower.layer = 14;
        towerComponent = tower.GetComponent<Tower_Detactor>();
        towerComponent.Init(null);
        SettingFog();

        occupySlider = Instantiate(Resources.Load("Prefabs/OccupySlider") as GameObject, transform);
        circularSlider = occupySlider.GetComponent<CircularSlider>();
    }
	
	// Update is called once per frame
	void Update () {
        if (rebuilding) return;
        if (towerComponent.IsDestroyed && !startSeize) {
            startSeize = true;
            coroutine = FindOwner();
            StartCoroutine(coroutine);
        }

        if (targets.Count == 0) {
            if (coroutine != null) {
                isAlreadyCoroutine = false;
                StopCoroutine(coroutine);
                circularSlider.Reset();
            }
            startSeize = false;
        }
	}

    public override void DestroyEnteredTarget(GameObject unitObj) {
        if (targets.Contains(unitObj)) {
            targets.Remove(unitObj);
            if (towerComponent.Enemy == unitObj.transform)
                towerComponent.Enemy = null;
        }
    }

    IEnumerator FindOwner() {
        if (isAlreadyCoroutine) StopCoroutine(coroutine);

        int targetLayer = 0;
        int time = 0;
        isAlreadyCoroutine = true;

        while (startSeize) {
            if(targets.Count == 0) {
                startSeize = false; 
                break;
            }
            foreach (GameObject target in targets.ToList()) {
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

            if (canSeize()) {
                time++;
                circularSlider.ChangeByValue(time);

                if (time == 100) {
                    OwnerNum = (PlayerController.Player)targetLayer;
                    GetComponent<Collider2D>().enabled = false;
                    targets.Clear();
                    GetComponent<Collider2D>().enabled = true;
                    StartCoroutine(RebuildTower());
                    startSeize = false;
                }
            }
        }
    }

    IEnumerator RebuildTower() {
        rebuilding = true;
        yield return new WaitForSeconds(8.0f);
        Building = Resources.Load("Prefabs/Tower") as GameObject;
        GameObject tower = Instantiate(Building, transform);
        tower.transform.SetAsFirstSibling();
        Destroy(towerComponent.gameObject);
        tower.layer = (int)OwnerNum;
        towerComponent = tower.GetComponent<Tower_Detactor>();
        rebuilding = false;
    }

    bool canSeize() {
        var query =
            from unit in targets
            where (unit.GetComponent<HeroAI>() != null)
            group unit by unit.GetComponent<UnitAI>().ownerNum;

        int count = 0;
        foreach (var group in query) {
            count++;
        }
        //Debug.Log("그룹 : " + count);
        if (count == 1) {
            //Debug.Log("점령 진행");
            return true;
        }
        else {
            //Debug.Log("적이 감지되어 점령 중지");
            return false;
        }
    }
}

public partial class TowerStation : DefaultStation {
    [SerializeField] [ReadOnly] public List<GameObject> targets;
    public Tower_Detactor towerComponent;

    public GameObject TowerObject {
        get {
            if (towerComponent != null)
                return towerComponent.transform.gameObject;
            else
                return null;
        }
    }


    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == 16 || collision.gameObject.layer == 5) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) {
                targets.Add(collision.gameObject);
                if (towerComponent.Enemy == null) towerComponent.Enemy = targets[0].transform;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 16 || collision.gameObject.layer == 5) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            targets.Remove(collision.gameObject);

            if(towerComponent.Enemy = collision.transform) {
                if (targets.Count <= 0) towerComponent.Enemy = null;
                else towerComponent.Enemy = targets[0].transform;
            }

        }
    }
}
