using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTower : MonoBehaviour {
    List<GameObject> targets;

    public Transform monsterParent;
    public List<GameObject> monsters;
    public Pool[] pools;
    int poolLv = 0;
    // Use this for initialization
    void Start() {
        MonstersReset(false);
    }

    // Update is called once per frame
    void Update() {

    }

    void OnTriggerStay2D(Collider2D collision) {
        if (collision.gameObject.layer == 11 && collision.GetComponent<UnitAI>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }

        if (collision.gameObject.layer == 9 && collision.GetComponent<BuildingObject>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 11 && collision.GetComponent<UnitAI>() != null) {
            targets.Remove(collision.gameObject);
        }

        if (collision.gameObject.layer == 9 && collision.GetComponent<BuildingObject>() != null) {
            targets.Remove(collision.gameObject);
        }
    }

    public void TargetDie() {

    }

    public void MonsterDie(GameObject monster) {
        monsters.Remove(monster);
    }

    public void MonstersReset(bool isTierUp) {
        monsters = new List<GameObject>();
        if (isTierUp) poolLv++;

        Pool selPool = pools[poolLv];

        foreach(Set set in selPool.sets) {
            for(int i=0; i<set.num; i++) {
                GameObject instantiatedMonster = Instantiate(set.monster, monsterParent);
                instantiatedMonster.transform.position = transform
                    .GetChild(0)
                    .GetChild(i)
                    .transform
                    .position;
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
