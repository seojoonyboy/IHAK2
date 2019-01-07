using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public GameObject setObject;
    Vector3 startPosition;
    Camera cam;

    private void Start() {
        Input.simulateMouseWithTouches = true;
        cam = Camera.main;
    }

    public void OnBeginDrag (PointerEventData eventData) {
        GetComponentInParent<DropHandler>().setObject = setObject;                
        startPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData) {    
        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null) {
            GameObject tile = hit.transform.gameObject;            
            transform.position = cam.WorldToScreenPoint(tile.transform.position);
            Debug.Log(transform.position);
            GetComponent<Image>().sprite = setObject.GetComponent<BuildingObject>().mainSprite;
        }
        else {
            GetComponent<Image>().sprite = setObject.GetComponent<BuildingObject>().icon;
            transform.position = Input.mousePosition;
        } 

    }

    public void OnEndDrag(PointerEventData eventData) {        
        transform.localPosition = Vector3.zero;
        GetComponent<Image>().sprite = setObject.GetComponent<BuildingObject>().icon;
    }

}
