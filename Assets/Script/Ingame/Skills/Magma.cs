using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Magma : MonoBehaviour {
    public SpriteRenderer range_texture;

    List<GameObject> targets;
    Data data;
    // Use this for initialization
    void Start() {
        targets = new List<GameObject>();
    }

    public void StartDamaging() {
        StartCoroutine(Damage(data.interval, data.duration));
    }

    public void Init(string[] data) {
        range_texture.transform.localScale = new Vector3(22, 22, 1);
        
        int range = 0;
        int.TryParse(data[0], out range);
        range /= 4;
        GetComponent<CircleCollider2D>().radius = range;
        range_texture.transform.localScale *= range;

        int interval = 1;

        int duration = 0;
        int.TryParse(data[1], out duration);

        int damage = 0;
        int.TryParse(data[2], out damage);

        this.data = new Data() {
            interval = interval,
            range = range,
            amount = damage,
            duration = duration
        };
    }

    IEnumerator Damage(float interval, int loopCount) {
        int count = 0;
        while (count < loopCount) {
            foreach(GameObject target in targets.ToList()) {
                if (target == null) continue;
                if(target.layer == 11 || target.layer == 14) {
                    target.GetComponent<AI.SkyNet>().Damage(data.amount);
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

    public struct Data {
        public int range;       //범위
        public int interval;    //간격
        public int amount;      //피해 정도
        public int duration;    //지속시간
    }
}
