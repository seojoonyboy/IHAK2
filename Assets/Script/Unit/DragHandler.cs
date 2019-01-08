using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    DropHandler dropHandler;
    public GameObject setObject;
    Vector3 startScale;
    Vector3 startPosition;
    Camera cam;
    

    private void Start() {
        dropHandler = GetComponentInParent<DropHandler>();
        Input.simulateMouseWithTouches = true;
        cam = Camera.main;
        startScale = transform.localScale;
    }

    

    public void OnBeginDrag (PointerEventData eventData) {
        dropHandler.setObject = setObject;                
        startPosition = transform.position;
        cam.GetComponent<BitBenderGames.MobileTouchCamera>().enabled = false;
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
            transform.localScale = startScale;
        } 

    }

    public void OnEndDrag(PointerEventData eventData) {        
        transform.localPosition = Vector3.zero;
        GetComponent<Image>().sprite = setObject.GetComponent<BuildingObject>().icon;
        cam.GetComponent<BitBenderGames.MobileTouchCamera>().enabled = true;
    }

}
