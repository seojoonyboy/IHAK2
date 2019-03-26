using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MagmaDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    Vector3 startScale;
    Vector3 startPosition;

    [SerializeField] [ReadOnly] GameObject 
        magmaPref,
        magma;
    [SerializeField] [ReadOnly] Camera camera;
    [SerializeField] [ReadOnly] Transform parent;
    [SerializeField] [ReadOnly] bool isInit = false;
    [SerializeField] [ReadOnly] GameObject parentBuilding;

    public void Init(Camera camera, GameObject magma, Transform parent, GameObject parentBuilding) {
        this.camera = camera;
        magmaPref = magma;
        this.parentBuilding = parentBuilding;
        isInit = true;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if(!isInit) {
            Debug.LogError("Magma의 초기화가 정상적으로 되지 않았습니다!");
            return;
        }

        if (magma == null) magma = Instantiate(magmaPref, parent);

        startPosition = transform.position;
        startScale = transform.localScale;
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;

        if (!isInit) {
            Debug.LogError("마그마 관련 초기화가 정상적으로 되지 않았습니다!");
            return;
        }

        magma.SetActive(true);
        Vector3 origin = camera.ScreenToWorldPoint(Input.mousePosition);
        magma.transform.position = new Vector3(origin.x, origin.y, 0);

    }

    public void OnEndDrag(PointerEventData eventData) {
        transform.position = startPosition;
        transform.localScale = new Vector3(1, 1, 1);

        magma.GetComponent<Magma>().StartDamaging();

        ActiveCardCoolTime coolComp = parentBuilding.AddComponent<ActiveCardCoolTime>();
        coolComp.targetCard = gameObject;
        coolComp.coolTime = 25;
        coolComp.behaviour = this;
        coolComp.StartCool();

        GetComponent<MagmaDragHandler>().enabled = false;
    }
}