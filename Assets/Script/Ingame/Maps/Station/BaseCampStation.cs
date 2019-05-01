using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class BaseCampStation : DefaultStation {

    [SerializeField] [ReadOnly] protected bool startSeize = false;
    [SerializeField] [ReadOnly] protected bool rebuilding = false;


    // Use this for initialization
    void Start () {
        OwnerNum = PlayerController.Player.NEUTRAL;
        StationIdentity = StationBasic.StationState.BaseCamp;
        creepList = new List<GameObject>();
        targets = new List<GameObject>();
        Building = Resources.Load("Prefabs/FowardHQ") as GameObject;
        GameObject tower = Instantiate(Building, transform);
        tower.layer = 14;
        towerComponent = tower.GetComponent<FowardHQ>();
        SetMonsters();
    }


    // Update is called once per frame
    void Update() {
        if (rebuilding) return;
        if (towerComponent.IsDestroyed && creepList.Count == 0 && !startSeize) {
            startSeize = true;
            StartCoroutine(FindOwner());
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
        int targetLayer = 0;
        while (startSeize) {
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
            OwnerNum = (PlayerController.Player)targetLayer;
            GetComponent<Collider2D>().enabled = false;
            targets.Clear();
            GetComponent<Collider2D>().enabled = true;
            StartCoroutine(RebuildTower());
            startSeize = false;
        }
    }

    IEnumerator RebuildTower() {
        rebuilding = true;
        yield return new WaitForSeconds(10.0f);
        Building = Resources.Load("Prefabs/FowardHQ") as GameObject;
        GameObject tower = Instantiate(Building, transform);
        tower.transform.SetAsFirstSibling();
        Destroy(towerComponent.gameObject);
        tower.layer = (int)OwnerNum;
        towerComponent = tower.GetComponent<FowardHQ>();
        rebuilding = false;
    }


}

public partial class BaseCampStation {
    public Transform monsterParent;
    public List<GameObject> monsters;
    public GameObject goblin;
    List<Transform> wayPoints = new List<Transform>();

    public void SetMonsters() {
        monsters = new List<GameObject>();
        monsterParent = transform.parent.parent.Find("Monsters");
        Instantiate(Resources.Load("Prefabs/Monsters/MonsterPos") as GameObject, transform);
        goblin = Resources.Load("Prefabs/Monsters/Goblin") as GameObject;

        goblin.GetComponent<MonsterAI>().Init(AccountManager.Instance.neutralMonsterDatas.Find(x => x.id == "npc_monster_01001"));
        goblin.GetComponent<MonsterAI>().expPoint = 20;

        foreach(Transform wayPoint in transform.GetChild(1)) {
            wayPoints.Add(wayPoint);
        }

        var creeps = AccountManager.Instance.mission.creeps;
        foreach(DataModules.MonsterData monsterData in creeps) {
            if(monsterData.creep.id == "npc_monster_01001") {
                int monsterCount = monsterData.count;
                for (int i = 0; i < monsterCount; i++) {
                    GameObject generateMonster = Instantiate(goblin, monsterParent);
                    generateMonster.transform.position = transform.GetChild(1).GetChild(i).position;

                    generateMonster.GetComponent<StateController>().SetupAI(true, wayPoints);
                    generateMonster.GetComponent<MonsterAI>().tower = this;
                    monsters.Add(generateMonster);
                }
            }
        }
    }

    public void MonsterDie(GameObject monster) {
        monsters.Remove(monster);
    }

}


public partial class BaseCampStation : DefaultStation {
    [SerializeField] [ReadOnly] public List<GameObject> targets;
    List<GameObject> creepList;
    public FowardHQ towerComponent;

    public GameObject TowerObject {
        get {
            if (towerComponent != null)
                return towerComponent.transform.gameObject;
            else
                return null;
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == 16) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) {
                targets.Add(collision.gameObject);
                if (towerComponent.Enemy == null) towerComponent.Enemy = targets[0].transform;
            }
        }

        if (collision.GetComponent<UnitGroup>() != null && (collision.transform.GetChild(0).gameObject.layer == (int)OwnerNum)) {
            collision.gameObject.AddComponent<RespawnMinion>();
        }
    }
    


    void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 16) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            targets.Remove(collision.gameObject);

            if(towerComponent.Enemy = collision.transform) {
                if (targets.Count <= 0) towerComponent.Enemy = null;
                else towerComponent.Enemy = targets[0].transform;
            }
        }

        if (collision.GetComponent<UnitGroup>() != null && (collision.transform.GetChild(0).gameObject.layer == (int)OwnerNum)) {
            Destroy(collision.gameObject.GetComponent<RespawnMinion>());
        }
    }    
}
