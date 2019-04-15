using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CreepStation : DefaultStation {
    bool hasOwner;

    // Use this for initialization
    void Start () {
        PlayerNum = PlayerController.Player.NEUTRAL;
        StationIdentity = StationBasic.StationState.Creep;
        targets = new List<GameObject>();
        hasOwner = false;

        MonstersReset(false);
    }

    private void LateUpdate() {
        if (!hasOwner && monsters.Count == 0 ) {
            hasOwner = true;
            StartCoroutine(FindOwner());
        }
    }

    IEnumerator FindOwner() {
        while (true) {
            bool player1 = false;
            bool player2 = false;
            foreach (GameObject target in targets) {
                if (target == null) continue;
                if (target.layer == 10) {
                    player1 = true;
                }
                else if (target.layer == 11) {
                    player2 = true;
                }
            }
            if (player1 != player2) {
                if (player1)
                    PlayerNum = PlayerController.Player.PLAYER_1;
                else
                    PlayerNum = PlayerController.Player.PLAYER_2;
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
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
        if ((collision.gameObject.layer == 10 || collision.gameObject.layer == 11) && collision.GetComponent<UnitAI>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if ((collision.gameObject.layer == 10 || collision.gameObject.layer == 11) && collision.GetComponent<UnitAI>() != null) {
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