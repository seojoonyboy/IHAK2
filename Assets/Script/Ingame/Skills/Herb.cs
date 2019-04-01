using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Herb : MonoBehaviour {
    public SpriteRenderer range_texture;
    private CircleCollider2D circleCollider;

    List<GameObject> targets;
    Data data;
    // Use this for initialization
    void Start() {
        targets = new List<GameObject>();

        circleCollider = GetComponent<CircleCollider2D>();
        data = Generate(new Data {
            range = 35,
            amount = 15
        });
    }

    public Data Generate(Data data) {
        circleCollider.radius = data.range;
        range_texture.transform.localScale *= data.range;
        return data;
    }

    public void StartHealing() {
        StartCoroutine(Heal(data.amount));
    }

    IEnumerator Heal(int amout) {
        foreach (GameObject target in targets.ToList()) {
            target.GetComponent<UnitAI>().health *= (15.0f / 100 + 1.0f);
        }
        yield return new WaitForSeconds(1.0f);
    }

    public struct Data {
        public int range;       //범위
        public int amount;      //%량
    }
}
