using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Magma : MonoBehaviour {
    public SpriteRenderer range_texture;
    private CircleCollider2D circleCollider;

    List<GameObject> targets = new List<GameObject>();

    // Use this for initialization
    void Start() {
        circleCollider = GetComponent<CircleCollider2D>();
        Generate(new Data {
            range = 45,
            amount = 30,
            interval = 1,
            duration = 6
        });
    }

    public void Generate(Data data) {
        circleCollider.radius = data.range;
        range_texture.transform.localScale *= data.range;
    }

    public void StartDamaging() {
        StartCoroutine(Damage(1, 6));
    }

    IEnumerator Damage(float interval, int loopCount) {
        int count = 0;
        while (count < loopCount) {
            count++;
            Debug.Log("마그마의 데미지 부여");
            yield return new WaitForSeconds(interval);
        }
        gameObject.SetActive(false);
    }

    void OnTriggerStay2D(Collider2D collision) {
        //Debug.Log(collision.gameObject.name + "감지됨");
        if (collision.gameObject.layer == 9 || collision.gameObject.layer == 11) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 9 || collision.gameObject.layer == 11) {
            targets.Remove(collision.gameObject);
        }
    }

    public void ActivateSpell() {
        Debug.Log("Magma Skill Activated");
        //Destroy(gameObject);
    }

    public struct Data {
        public int range;       //범위
        public int interval;    //간격
        public int amount;      //초당 
        public int duration;    //지속시간
    }
}
