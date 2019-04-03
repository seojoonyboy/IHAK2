using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScaryOracle : MonoBehaviour {
    public SpriteRenderer range_texture;

    List<GameObject> targets;
    Data data;

    public void StartDebuff() {
        StartCoroutine(Debuff());
    }

    IEnumerator Debuff() {
        int count = 0;
        while(count < data.duration) {
            foreach(GameObject target in targets.ToList()) {
                if (target == null) continue;
                if (target.layer == 11) {
                    target.GetComponent<UnitAI>().ScaryOracle(data.amount);
                }
            }
            count++;
            yield return new WaitForSeconds(data.interval);
        }
    }

    public void Init(string data) {
        range_texture.transform.localScale = new Vector3(2.336628f, 2.336628f, 2.336628f);
        targets = new List<GameObject>();

        this.data = new Data() {
            interval = 1,
            range = 45,
            amount = 65,
            duration = 6
        };
    }

    void OnTriggerStay2D(Collider2D collision) {
        if (collision.gameObject.layer == 11) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 11) {
            targets.Remove(collision.gameObject);
        }
    }

    public struct Data {
        public int range;       //범위
        public int interval;    //간격
        public int amount;      //피해 정도
        public int duration;    //지속시간
    }
}
