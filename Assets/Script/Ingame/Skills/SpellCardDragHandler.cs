using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpellCardDragHandler : IngameActiveCardDragHandler {
    [SerializeField] protected GameObject prefab, obj;
    [SerializeField] protected Camera camera;
    [SerializeField] protected Transform parent;
    [SerializeField] protected bool isInit = false;
    [SerializeField] protected GameObject parentBuilding;
    [SerializeField] protected string[] data;
    [SerializeField] protected int coolTime;

    void Start() {
        base.Init();
    }

    public override void OnBeginDrag(PointerEventData eventData) {
        base.OnBeginDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData) {
        base.OnEndDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData) {
        base.OnDrag(eventData);

        if (!isInit) {
            Debug.LogError("Prefab 관련 초기화가 정상적으로 되지 않았습니다!");
            return;
        }

        if (obj != null) {
            obj.SetActive(true);
            Vector3 origin = camera.ScreenToWorldPoint(Input.mousePosition);
            obj.transform.position = new Vector3(origin.x, origin.y, 0);
        }
    }

    public void Setting() {
        if (!isInit) {
            Debug.LogError("Prefab의 초기화가 정상적으로 되지 않았습니다!");
            return;
        }

        if (obj == null) obj = Instantiate(prefab, parent);

        startPosition = transform.position;
        startScale = transform.localScale;
    }

    public void Init(Camera camera, GameObject prefab, Transform parent, GameObject parentBuilding, IngameDeckShuffler deckShuffler, string[] data, int coolTime) {
        this.camera = camera;
        this.prefab = prefab;
        this.parentBuilding = parentBuilding;
        this.deckShuffler = deckShuffler;
        this.data = data;
        this.coolTime = coolTime;
        isInit = true;
    }
}
