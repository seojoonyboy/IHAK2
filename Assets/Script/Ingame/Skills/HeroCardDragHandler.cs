using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Sirenix.OdinInspector;
using BitBenderGames;

public class HeroCardDragHandler : IngameActiveCardDragHandler {
    public GameObject instantiatedUnitObj;
    public List<Vector3> path;

    void Start() {
        base.Init();

        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(() => OnPointerClick());
    }

    public new void CancelDrag() {
        base.CancelDrag();
        GetComponent<HeroCardDragHandler>().enabled = false;

        transform.position = startPosition;
        transform.localScale = new Vector3(1, 1, 1);
        
        foreach (Text list in transform.GetComponentsInChildren<Text>()) list.enabled = true;
        foreach (Image image in transform.GetComponentsInChildren<Image>()) if (image.name != "Image") image.enabled = true;

        GameObject deactive = transform.Find("Deactive").gameObject;
        deactive.SetActive(true);
    }

    public override void OnBeginDrag(PointerEventData eventData) {
        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData) {
        base.OnDrag(eventData);

        GraphicRaycaster m_Raycaster = GetComponentInParent<GraphicRaycaster>();
        PointerEventData m_PointEventData = new PointerEventData(FindObjectOfType<EventSystem>());
        m_PointEventData.position = Input.mousePosition;

        transform.GetComponent<Image>().enabled = false;
        foreach (Text list in transform.GetComponentsInChildren<Text>()) list.enabled = false;
        foreach (Image image in transform.GetComponentsInChildren<Image>()) if (image.name != "Portrait") image.enabled = false;

        GetComponentInChildren<IngameModule.PathPreviewInDrag>().OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData) {
        base.OnEndDrag(eventData);

        GraphicRaycaster m_Raycaster = GetComponentInParent<GraphicRaycaster>();
        PointerEventData m_PointEventData = new PointerEventData(FindObjectOfType<EventSystem>());
        m_PointEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointEventData, results);

        GetComponentInChildren<IngameModule.PathPreviewInDrag>().OnEndDrag(eventData);

        transform.localScale = new Vector3(1, 1, 1);

        transform.GetComponent<Image>().enabled = true;
        foreach(Text list in transform.GetComponentsInChildren<Text>()) list.enabled = true;
        foreach (Image image in transform.GetComponentsInChildren<Image>()) if (image.name != "Portrait") image.enabled = true;

        GetComponentInChildren<BoundaryCamMove>().isDrag = false;

        if (eventData == null) return;

        ActiveCardCoolTime coolComp = GetComponent<ActiveCardInfo>().data.parentBuilding.GetComponent<ActiveCardCoolTime>();
        if (coolComp != null) {
            Debug.Log("쿨타임! 사용불가");
            return;
        }

        foreach(RaycastResult result in results){
            if (result.gameObject.name == "HeroCards") { return; }
        }

        if(instantiatedUnitObj != null) {
            instantiatedUnitObj.GetComponentInParent<UnitGroup>().SetMove(path);
        }
        else {
            deckShuffler.UseCard(gameObject);
        }
    }

    public void OnPointerClick() {
        if(instantiatedUnitObj != null) {
            iTween.MoveTo(
                cam.gameObject, 
                new Vector3(
                    instantiatedUnitObj.transform.position.x,
                    instantiatedUnitObj.transform.position.y,
                    cam.transform.position.z
                ),
                1.0f
            );
        }
    }
}
