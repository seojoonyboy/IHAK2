using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public GameObject unit;
    public int unitID;
    Vector3 startPosition;

    private void Start() {
        Input.simulateMouseWithTouches = true;
    }

    public void OnBeginDrag (PointerEventData eventData) {
        GetComponentInParent<UnitDropHandler>().unit = unit;
        GetComponentInParent<UnitDropHandler>().selectID = unitID;
        
        GetComponent<Image>().sprite = unit.GetComponent<BuildingObject>().mainSprite;        
        startPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {        
        transform.localPosition = Vector3.zero;
        GetComponent<Image>().sprite = unit.GetComponent<BuildingObject>().icon;
    }	
}
