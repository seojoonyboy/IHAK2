using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    public void Init(string data) {
        range_texture.transform.localScale = new Vector3(2.336628f, 2.336628f, 2.336628f);

        string[] args = data.Split(',');

        int range = 0;
        int.TryParse(args[0], out range);
        GetComponent<CircleCollider2D>().radius = range;
        range_texture.transform.localScale *= range;

        int interval = 1;

        int duration = 0;
        int.TryParse(args[1], out duration);

        int damage = 0;
        int.TryParse(args[2], out damage);

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
                if(target.layer == 11) {
                    target.GetComponent<UnitAI>().damaged(data.amount);
                }
                else if(target.layer == 9) {
                    int tileNum = target.GetComponent<BuildingObject>().setTileLocation;
                    IngameHpSystem.Instance.TakeDamage(IngameHpSystem.Target.ME, tileNum, 30);
                    //target.GetComponent<>
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

        if(collision.gameObject.layer == 9 && collision.GetComponent<BuildingObject>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if(collision.gameObject.layer == 11 && collision.GetComponent<UnitAI>() != null) {
            targets.Remove(collision.gameObject);
        }

        if (collision.gameObject.layer == 9 && collision.GetComponent<BuildingObject>() != null) {
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
