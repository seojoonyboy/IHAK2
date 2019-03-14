using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HealArea : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    IngameDropHandler dropHandler;
    Vector3 startScale;
    Vector3 startPosition;
    public Camera cam;
    public GameObject pref;
    GameObject detector;
    // Use this for initialization
    void Start() {
        dropHandler = GetComponentInParent<IngameDropHandler>();
    }

    void OnDetector() {
        var skill = GetComponent<ActiveCardInfo>().data.baseSpec.skill;
        GameObject collObj = new GameObject();

        detector = Instantiate(pref);
        detector.name = "heal_detector";
        detector.GetComponent<Heal_detector>().Init(skill);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        OnDetector();

        dropHandler.selectedObject = gameObject;
        dropHandler.selectedObject.GetComponent<Image>().raycastTarget = false;
        startPosition = transform.position;
        startScale = transform.localScale;
    }

    public void CancelDrag() {
        transform.position = startPosition;
        transform.localScale = new Vector3(1, 1, 1);
        if (dropHandler == null || dropHandler.selectedObject == null) return;
        dropHandler.selectedObject.GetComponent<Image>().raycastTarget = true;

        foreach (Text list in transform.GetComponentsInChildren<Text>()) list.enabled = true;
        foreach (Image image in transform.GetComponentsInChildren<Image>()) if (image.name != "Image") image.enabled = true;
        OnEndDrag(null);
    }

    public void OnDrag(PointerEventData eventData) {
        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        detector.transform.position = new Vector3(origin.x, origin.y, 0);

        transform.position = Input.mousePosition;

        GraphicRaycaster m_Raycaster = GetComponentInParent<GraphicRaycaster>();
        PointerEventData m_PointEventData = new PointerEventData(FindObjectOfType<EventSystem>());
        m_PointEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointEventData, results);
        if (results[0].gameObject.name.CompareTo("Horizontal Scroll Snap") != 0) return;

        transform.GetComponent<Image>().enabled = false;
        foreach (Text list in transform.GetComponentsInChildren<Text>()) list.enabled = false;
        foreach (Image image in transform.GetComponentsInChildren<Image>()) if (image.name != "Image") image.enabled = false;
    }

    public void OnEndDrag(PointerEventData eventData) {
        transform.position = startPosition;
        transform.localScale = new Vector3(1, 1, 1);
        dropHandler.selectedObject.GetComponent<Image>().raycastTarget = true;
        transform.GetComponent<Image>().enabled = true;
        foreach (Text list in transform.GetComponentsInChildren<Text>()) list.enabled = true;
        foreach (Image image in transform.GetComponentsInChildren<Image>()) if (image.name != "Image") image.enabled = true;

        if (eventData == null) {
            Destroy(detector);
            return;
        }

        ActiveCardCoolTime coolComp = GetComponent<ActiveCardInfo>().data.parentBuilding.GetComponent<ActiveCardCoolTime>();
        if (coolComp != null) {
            Debug.Log("쿨타임! 사용불가");
            return;
        }

        detector.GetComponent<Heal_detector>().ActivateSpell();
        dropHandler.OnDrop();
    }
}
