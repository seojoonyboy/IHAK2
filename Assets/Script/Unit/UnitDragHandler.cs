using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public GameObject unit;
    Vector3 startPosition;
    bool click;

    private void Start() {
        Input.simulateMouseWithTouches = true;
    }

    public void OnBeginDrag (PointerEventData eventData) {
        GetComponentInParent<UnitDropHandler>().unit = unit;
        startPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {        
        transform.localPosition = Vector3.zero;
    }	
}
