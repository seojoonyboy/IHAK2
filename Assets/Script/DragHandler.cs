using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public static GameObject unit;
    Vector3 startPosition;
    Transform startParent;

	public void OnBeginDrag(PointerEventData eventData) {
        unit = gameObject;
        startPosition = transform.position;
        startParent = transform.parent;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
        unit = null;
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        //if (transform.parent != startParent)
            transform.position = startPosition;
    }
}
