using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public GameObject setObject;
    Vector3 startPosition;

    private void Start() {
        Input.simulateMouseWithTouches = true;
    }

    public void OnBeginDrag (PointerEventData eventData) {

        GetComponentInParent<DropHandler>().setObject = setObject;
        GetComponent<Image>().sprite = setObject.GetComponent<BuildingObject>().mainSprite;        
        startPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {        
        transform.localPosition = Vector3.zero;
        GetComponent<Image>().sprite = setObject.GetComponent<BuildingObject>().icon;
    }	
}
