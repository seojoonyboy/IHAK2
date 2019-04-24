using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Magma : MonoBehaviour {
    List<GameObject> targets;
    // Use this for initialization
    void Start() {
        targets = new List<GameObject>();
    }

    public void StartDamaging() {
        SpellCardDragHandler hanlder = GetComponent<SpellCardDragHandler>();
        StartCoroutine(Damage(1, 6));
    }

    IEnumerator Damage(float interval, int loopCount) {
        int count = 0;
        while (count < loopCount) {
            foreach(GameObject target in targets.ToList()) {
                if (target == null) continue;
                if(target.layer == 11 || target.layer == 14) {
                    target.GetComponent<AI.SkyNet>().Damage(12);
                }
            }
            count++;
            yield return new WaitForSeconds(interval);
        }
        gameObject.SetActive(false);
    }

    void OnTriggerStay2D(Collider2D collision) {
        if(collision.gameObject.layer == 11 && collision.GetComponent<UnitAI>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }

        if(collision.gameObject.layer == 14 && collision.GetComponent<MonsterAI>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if(collision.gameObject.layer == 11 && collision.GetComponent<UnitAI>() != null) {
            targets.Remove(collision.gameObject);
        }
        if (collision.gameObject.layer == 14 && collision.GetComponent<MonsterAI>() != null) {
            targets.Remove(collision.gameObject);
        }
    }
}
