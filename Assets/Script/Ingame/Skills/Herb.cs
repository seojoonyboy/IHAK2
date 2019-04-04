using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Herb : MonoBehaviour {
    public SpriteRenderer range_texture;

    List<GameObject> targets;
    Data data;
    // Use this for initialization
    void Start() {
        targets = new List<GameObject>();
    }

    public void Init(string data) {
        range_texture.transform.localScale = new Vector3(2.336628f, 2.336628f, 2.336628f);

        string[] args = data.Split(',');

        this.data = new Data();
        int.TryParse(args[0], out this.data.range);
        int.TryParse(args[1], out this.data.amount);

        GetComponent<CircleCollider2D>().radius = this.data.range;
        range_texture.transform.localScale *= this.data.range;
    }

    public void StartHealing() {
        StartCoroutine(Heal(data.amount));
    }

    IEnumerator Heal(int amout) {
        int count = 0;
        while(count < 5) {
            foreach (GameObject target in targets.ToList()) {
                count++;
                if (target == null) continue;

                target.GetComponent<UnitAI>().HerbRation(data.amount);
                yield return new WaitForSeconds(1.0f);
            }
        }
        gameObject.SetActive(false);
    }

    public struct Data {
        public int range;       //범위
        public int amount;      //%량
    }

    void OnTriggerStay2D(Collider2D collision) {
        if (collision.gameObject.layer == 10 && collision.GetComponent<UnitAI>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 10 && collision.GetComponent<UnitAI>() != null) {
            targets.Remove(collision.gameObject);
        }
    }
}
