using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler {
    DropHandler dropHandler;
    public GameObject setObject;
    Vector3 startScale;
    Vector3 startPosition;
    float camMagnification;
    Camera cam;
    private bool canDrag = false;

    public void BeginDrag() {
        canDrag = true;
        OnBeginDrag(null);
    }

    private void Start() {
        dropHandler = GetComponentInParent<DropHandler>();
        Input.simulateMouseWithTouches = true;
        cam = Camera.main;
        startScale = transform.localScale;

        GetComponent<LongClickButton>().onShortClick.AddListener(() => Debug.Log("Short Button Click"));
    }

    public void OnBeginDrag (PointerEventData eventData) {
        if (!canDrag) return;
        dropHandler.setObject = setObject;
        startPosition = transform.position;        
        camMagnification = (dropHandler.startCamSize - dropHandler.camSize) * 0.025f;
        cam.GetComponent<BitBenderGames.MobileTouchCamera>().enabled = false;
    }

    public void OnEndDrag() {
        transform.localPosition = Vector3.zero;
        transform.localScale = startScale;
        GetComponent<Image>().sprite = setObject.GetComponent<BuildingObject>().icon;
        cam.GetComponent<BitBenderGames.MobileTouchCamera>().enabled = true;

        canDrag = false;
        dropHandler.OnDrop();
    }

    public void OnDrag(PointerEventData eventData) {
        if (!canDrag) return;
        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null) {
            GameObject tile = hit.transform.gameObject;
            transform.localScale = new Vector3(startScale.x * 1.5f + camMagnification, startScale.y * 1.5f + camMagnification);
            transform.position = cam.WorldToScreenPoint(tile.transform.position);            
            GetComponent<Image>().sprite = setObject.GetComponent<BuildingObject>().mainSprite;
        }
        else {
            GetComponent<Image>().sprite = setObject.GetComponent<BuildingObject>().icon;
            transform.position = Input.mousePosition;
            transform.localScale = startScale;
        }
    }
}
