using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreepStation : DefaultStation {

    [SerializeField]
    List<GameObject> targets;
    List<GameObject> creeps;

    bool hasOwner;

    // Use this for initialization
    void Start () {
        PlayerNum = PlayerController.Player.NEUTRAL;
        StationIdentity = StationBasic.StationState.Creep;
        targets = new List<GameObject>();
        creeps = new List<GameObject>();
        hasOwner = false;
    }
	
	// Update is called once per frame
	void Update () {
	}

    private void LateUpdate() {
        if (!hasOwner && creeps.Count == 0 ) {
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

    void OnTriggerStay2D(Collider2D collision) {
        if (collision.gameObject.layer == 10 && collision.GetComponent<UnitAI>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }

        if (collision.gameObject.layer == 11 && collision.GetComponent<UnitAI>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 10 && collision.GetComponent<UnitAI>() != null) {
            targets.Remove(collision.gameObject);
        }

        if (collision.gameObject.layer == 11 && collision.GetComponent<UnitAI>() != null) {
            targets.Remove(collision.gameObject);
        }
    }
}
