using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class CreepStation : DefaultStation {

    [SerializeField] [ReadOnly] protected bool startSeize = false;

    // Use this for initialization
    void Start () {
        OwnerNum = PlayerController.Player.NEUTRAL;
        StationIdentity = StationBasic.StationState.Creep;
        targets = new List<GameObject>();
        SetMonsters();
        MonstersReset(false);
    }

    private void LateUpdate() {
        if (!startSeize && monsters.Count == 0 && targets.Count > 0 ) {
            startSeize = true;
            StartCoroutine(FindOwner());
        }
    }

    IEnumerator FindOwner() {
        int targetLayer = 0;
        while (startSeize) {
            foreach (GameObject target in targets) {
                if(target == null) continue;
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
            startSeize = false;
        }
    }

    public void ChangeOwner() { }
}

public partial class CreepStation {
    [SerializeField] [ReadOnly] public List<GameObject> targets;
    public Transform monsterParent;
    public List<GameObject> monsters;
    public Pool[] pools = new Pool[1];
    [SerializeField] [ReadOnly] int poolLv = 0;

    public void SetMonsters() {
        monsters = new List<GameObject>();
        monsterParent = transform.parent.parent.Find("Monsters");
        Instantiate(Resources.Load("Prefabs/Monsters/MonsterPos") as GameObject, transform);
        Set goblin = new Set {
            num = 6,
            monster = Resources.Load("Prefabs/Monsters/Goblin") as GameObject
        };

        goblin.monster.GetComponent<MonsterAI>().Init(AccountManager.Instance.neutralMonsterDatas.Find(x => x.id == "npc_monster_01001"));

        List<Set> tempSets = new List<Set>();
        tempSets.Add(goblin);
        Pool tempPool = new Pool();
        tempPool.sets = tempSets;
        pools[0] = tempPool;
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == 16) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 16) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            foreach(GameObject monster in monsters) {
                if(monster.GetComponent<StateController>().chaseTarget == collision.transform) {
                    monster.GetComponent<StateController>().chaseTarget = null;
                }
            }
            targets.Remove(collision.gameObject);
        }
    }

    void Update() {
        foreach (GameObject monster in monsters.ToList()) {
            if (monster == null) {
                monsters.Remove(monster);
            }
        }
        if (targets != null && targets.Count == 0) {
            foreach (GameObject monster in monsters) {
                monster.GetComponent<StateController>().chaseTarget = null;
            }
        }
    }

    public void TargetDie() { }

    public void MonsterDie(GameObject monster) {
        monsters.Remove(monster);
    }

    public void MonstersReset(bool isTierUp) {
        if (isTierUp) poolLv++;

        Pool selPool = pools[poolLv];

        List<Transform> wayPoints = new List<Transform>();
        foreach (Transform wayPoint in transform.GetChild(0)) {
            wayPoints.Add(wayPoint);
        }

        foreach (Set set in selPool.sets) {
            for (int i = 0; i < set.num; i++) {
                GameObject instantiatedMonster = Instantiate(set.monster, monsterParent);
                instantiatedMonster.transform.position = transform
                    .GetChild(0)
                    .GetChild(i)
                    .transform
                    .position;
                
                instantiatedMonster.GetComponent<StateController>().SetupAI(true, wayPoints);
                instantiatedMonster.GetComponent<MonsterAI>().tower = this;
                monsters.Add(instantiatedMonster);
            }
        }
    }

    

    [System.Serializable]
    public class Pool {
        public List<Set> sets;
    }

    [System.Serializable]
    public class Set {
        public int num;
        public GameObject monster;
    }
}