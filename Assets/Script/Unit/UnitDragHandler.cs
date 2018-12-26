using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public GameObject unit;
    Vector3 startPosition;

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
