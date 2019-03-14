using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HealArea : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    IngameDropHandler dropHandler;
    Vector3 startScale;
    Vector3 startPosition;
    Camera cam;

    CircleCollider2D collider;
    List<GameObject> targets;

    int healAmount = 0;
    int range = 0;
    // Use this for initialization
    void Start() {
        dropHandler = GetComponentInParent<IngameDropHandler>();
        cam = Camera.main;

        targets = new List<GameObject>();
    }

    public void OnDetector() {
        var skill = GetComponent<ActiveCardInfo>().data.baseSpec.skill;
        Init(skill);

        collider = gameObject.AddComponent<CircleCollider2D>();
        collider.radius = range * 10;
    }

    private void Init(DataModules.Skill skill) {
        string[] args = skill.method.args.Split(',');

        int.TryParse(args[0], out range);
        int.TryParse(args[1], out healAmount);
    }

    public void OffDetector() {
        Destroy(collider);
    }

    void OnTriggerStay2D(Collider2D collision) {
        Debug.Log(collider.gameObject.name + "감지됨");
        if (collision.gameObject.layer == 10) {
            targets.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if(collision.gameObject.layer == 10) {
            targets.Remove(collision.gameObject);
        }
    }

    public void ActivateSpell() {
        OffDetector();
        foreach(GameObject obj in targets) {
            UnitAI unitAI = obj.GetComponent<UnitAI>();
            if (unitAI != null) unitAI.health += healAmount;
        }
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
        OffDetector();

        transform.position = startPosition;
        transform.localScale = new Vector3(1, 1, 1);
        dropHandler.selectedObject.GetComponent<Image>().raycastTarget = true;
        transform.GetComponent<Image>().enabled = true;
        foreach (Text list in transform.GetComponentsInChildren<Text>()) list.enabled = true;
        foreach (Image image in transform.GetComponentsInChildren<Image>()) if (image.name != "Image") image.enabled = true;

        if (eventData == null) return;

        ActiveCardCoolTime coolComp = GetComponent<ActiveCardInfo>().data.parentBuilding.GetComponent<ActiveCardCoolTime>();
        if (coolComp != null) {
            Debug.Log("쿨타임! 사용불가");
            return;
        }
        ActivateSpell();
        dropHandler.OnDrop();
    }
}
