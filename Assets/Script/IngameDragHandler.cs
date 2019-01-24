using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IngameDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    IngameDropHandler dropHandler;
    Vector3 startScale;
    Vector3 startPosition;
    Camera cam;
    void Start() {
        dropHandler = GetComponentInParent<IngameDropHandler>();
        cam = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        dropHandler.selectedObject = gameObject;
        dropHandler.selectedObject.GetComponent<Image>().raycastTarget = false;
        GetComponent<DataModules.Index>().Id = transform.GetSiblingIndex();
        startPosition = transform.position;
        startScale = transform.localScale;
    }

    public void OnDrag(PointerEventData eventData) {
        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if(hit.collider != null) {
            if (hit.collider.tag == "BackGroundTile") {
                GameObject tile = hit.transform.gameObject;
                Vector3 position = cam.WorldToScreenPoint(origin);
                position.z = 0;
                transform.position = new Vector3(position.x, position.y + 100f, position.z);

                //Debug.Log(hit.collider.gameObject.layer);
            }
            else {
                transform.position = Input.mousePosition;
            }
            
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        transform.position = startPosition;
        transform.localScale = startScale;
        dropHandler.selectedObject.GetComponent<Image>().raycastTarget = true;
        Canvas.ForceUpdateCanvases();
        var hlg = transform.parent.GetComponent<HorizontalLayoutGroup>();
        hlg.CalculateLayoutInputHorizontal();
        hlg.CalculateLayoutInputVertical();
        hlg.SetLayoutHorizontal();
        hlg.SetLayoutVertical();

        dropHandler.OnDrop();
    }
}
