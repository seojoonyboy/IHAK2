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
        transform.position = Input.mousePosition;

        GraphicRaycaster m_Raycaster = GetComponentInParent<GraphicRaycaster>();
        PointerEventData m_PointEventData = new PointerEventData(FindObjectOfType<EventSystem>());
        m_PointEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointEventData, results);
        if(results[0].gameObject.name.CompareTo("Horizontal Scroll Snap")!=0) return;

        transform.GetComponent<Image>().enabled = false;
        foreach(Text list in transform.GetComponentsInChildren<Text>()) list.enabled = false;
    }

    public void OnEndDrag(PointerEventData eventData) {
        transform.position = startPosition;
        transform.localScale = startScale;
        dropHandler.selectedObject.GetComponent<Image>().raycastTarget = true;
        transform.GetComponent<Image>().enabled = true;
        foreach(Text list in transform.GetComponentsInChildren<Text>()) list.enabled = true;
        Canvas.ForceUpdateCanvases();
        var hlg = transform.parent.GetComponent<HorizontalLayoutGroup>();
        hlg.CalculateLayoutInputHorizontal();
        hlg.CalculateLayoutInputVertical();
        hlg.SetLayoutHorizontal();
        hlg.SetLayoutVertical();

        dropHandler.OnDrop();
    }
}
