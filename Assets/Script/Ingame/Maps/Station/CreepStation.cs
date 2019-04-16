using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CreepStation : DefaultStation {

    private bool startSeize = false;

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
                if (target == null) continue;
                if ((int)OwnerNum != target.layer) {
                    int tempLayer = targetLayer;
                    targetLayer = target.layer;
                    if (tempLayer != targetLayer) {
                        startSeize = false;
                    }
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
        List<Set> tempSets = new List<Set>();
        tempSets.Add(goblin);
        Pool tempPool = new Pool();
        tempPool.sets = tempSets;
        pools[0] = tempPool;
    }

    void OnTriggerStay2D(Collider2D collision) {
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            targets.Remove(collision.gameObject);
        }
    }

    public void TargetDie() { }

    public void MonsterDie(GameObject monster) {
        monsters.Remove(monster);
    }

    public void MonstersReset(bool isTierUp) {
        if (isTierUp) poolLv++;

        Pool selPool = pools[poolLv];

        foreach (Set set in selPool.sets) {
            for (int i = 0; i < set.num; i++) {
                GameObject instantiatedMonster = Instantiate(set.monster, monsterParent);
                instantiatedMonster.transform.position = transform
                    .GetChild(0)
                    .GetChild(i)
                    .transform
                    .position;
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