using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Magma : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public SpriteRenderer range_texture;
    private CircleCollider2D circleCollider;

    Vector3 startScale;
    Vector3 startPosition;

    // Use this for initialization
    void Start() {
        GetComponent<CircleCollider2D>();
        Generate(new Data {
            range = 45,
            amount = 30,
            interval = 1,
            duration = 6
        });
    }

    // Update is called once per frame
    void Update() {

    }

    public void Generate(Data data) {
        circleCollider.radius = data.range;
        range_texture.transform.localScale *= data.range;
    }

    IEnumerator Damage(float interval, int loopCount) {
        int count = 0;
        while(count > loopCount) {
            count++;
            yield return new WaitForSeconds(interval);
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        startPosition = transform.position;
        startScale = transform.localScale;
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
        transform.position = startPosition;
        transform.localScale = new Vector3(1, 1, 1);
    }

    public struct Data {
        public int range;       //범위
        public int interval;    //간격
        public int amount;      //초당 
        public int duration;    //지속시간
    }
}
