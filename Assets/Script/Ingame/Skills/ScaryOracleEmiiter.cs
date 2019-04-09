using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScaryOracleEmiiter : MonoBehaviour {
    public SpriteRenderer range_texture;

    List<GameObject> targets;
    Data data;

    public void StartDebuff() {
        StartCoroutine(Debuff());
    }

    IEnumerator Debuff() {
        int count = 0;
        while (count < data.duration) {
            foreach (GameObject target in targets.ToList()) {
                if (target == null) continue;
                if (target.layer == 11) {
                    //target.GetComponent<UnitAI>().ScaryOracle(data.amount);
                    if (target.GetComponent<ScaryOracle>() == null) {
                        ScaryOracle scaryOracle = target.AddComponent<ScaryOracle>();
                        scaryOracle.percentage = data.amount;
                    }
                }
            }
            count++;
            yield return new WaitForSeconds(data.interval);
        }

        foreach(GameObject target in targets.ToList()) {
            if (target == null) continue;
            if (target.layer == 11) {
                Destroy(target.GetComponent<ScaryOracle>());
            }
        }
        gameObject.SetActive(false);
    }

    public struct Data {
        public int range;       //범위
        public int interval;    //간격
        public int amount;      //피해 정도
        public int duration;    //지속시간
    }
    public void Init(string[] data) {
        //range_texture.transform.localScale = new Vector3(2.336628f, 2.336628f, 2.336628f);
        //targets = new List<GameObject>();

        //this.data = new Data() {
        //    interval = 1,
        //    range = 45,
        //    amount = 65,
        //    duration = 6
        //};

        //GetComponent<CircleCollider2D>().radius = this.data.range;
        //range_texture.transform.localScale *= this.data.range;
    }

    void OnTriggerStay2D(Collider2D collision) {
        if (collision.gameObject.layer == 11 && (collision.gameObject.GetComponent<HeroAI>() != null || collision.gameObject.GetComponent<MinionAI>() != null)) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 11 && (collision.gameObject.GetComponent<HeroAI>() != null || collision.gameObject.GetComponent<MinionAI>() != null)) {
            Destroy(collision.gameObject.GetComponent<ScaryOracle>());
            targets.Remove(collision.gameObject);
        }
    }
}
