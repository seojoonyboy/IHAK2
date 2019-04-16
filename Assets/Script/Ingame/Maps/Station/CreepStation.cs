using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CreepStation : DefaultStation {

    // Use this for initialization
    void Start () {
        OwnerNum = PlayerController.Player.NEUTRAL;
        StationIdentity = StationBasic.StationState.Creep;
        targets = new List<GameObject>();

        MonstersReset(false);
    }

    private void LateUpdate() {
        if (monsters.Count == 0 && targets.Count > 0 ) {
            StartCoroutine(FindOwner());
        }
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

    public void ChangeOwner() { }
}

public partial class CreepStation {
    [SerializeField] [ReadOnly] public List<GameObject> targets;
    public Transform monsterParent;
    public List<GameObject> monsters;
    public Pool[] pools;
    [SerializeField] [ReadOnly] int poolLv = 0;
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
        monsters = new List<GameObject>();
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