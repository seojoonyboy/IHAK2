using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Sirenix.OdinInspector;
using BitBenderGames;

public class HeroCardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    IngameSceneEventHandler eventHandler;
    [SerializeField] [ReadOnly] IngameDeckShuffler deckShuffler;

    Vector3 startScale;
    Vector3 startPosition;
    Camera cam;
    public GameObject instantiatedUnitObj;

    void Awake() {
        eventHandler = IngameSceneEventHandler.Instance;
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.BUILDING_DESTROYED, BuildingDestroyed);
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.BUILDING_RECONSTRUCTED, BuildingReconstucted);
    }

    void Start() {
        cam = Camera.main;

        EventTrigger et = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;

        entry.callback.AddListener(
            (eventData) => cam.GetComponent<TouchInputController>().OnEventTriggerPointerDown(null)
        );

        et.triggers.Add(entry);

        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(() => OnPointerClick());

        deckShuffler = PlayerController.Instance.deckShuffler();
    }

    void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.BUILDING_DESTROYED, BuildingDestroyed);
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.BUILDING_RECONSTRUCTED, BuildingReconstucted);
    }

    private void BuildingDestroyed(Enum Event_Type, Component Sender, object Param) {
        //IngameSceneEventHandler.BuildingDestroyedPackage parms = (IngameSceneEventHandler.BuildingDestroyedPackage) Param;
        //if(parms.target == IngameHpSystem.Target.ME) {
        //    if(GetComponent<ActiveCardInfo>().data.parentBuilding == parms.buildingInfo.gameObject) {
        //        CancelDrag();
        //    }
        //}
    }

    private void BuildingReconstucted(Enum Event_Type, Component Sender, object Param) {
        //IngameSceneEventHandler.BuildingDestroyedPackage parms = (IngameSceneEventHandler.BuildingDestroyedPackage)Param;
        //if (parms.target == IngameHpSystem.Target.ME) {
        //    if (GetComponent<ActiveCardInfo>().data.parentBuilding == parms.buildingInfo.gameObject) {
        //        GetComponent<IngameDragHandler>().enabled = true;
        //    }
        //}
    }

    public void CancelDrag() {
        GetComponent<HeroCardDragHandler>().enabled = false;

        transform.position = startPosition;
        transform.localScale = new Vector3(1, 1, 1);
        
        foreach (Text list in transform.GetComponentsInChildren<Text>()) list.enabled = true;
        foreach (Image image in transform.GetComponentsInChildren<Image>()) if (image.name != "Image") image.enabled = true;

        GameObject deactive = transform.Find("Deactive").gameObject;
        deactive.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        startPosition = transform.position;
        startScale = transform.localScale;

        GetComponentInChildren<BoundaryCamMove>().isDrag = true;
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;

        GraphicRaycaster m_Raycaster = GetComponentInParent<GraphicRaycaster>();
        PointerEventData m_PointEventData = new PointerEventData(FindObjectOfType<EventSystem>());
        m_PointEventData.position = Input.mousePosition;

        transform.GetComponent<Image>().enabled = false;
        foreach (Text list in transform.GetComponentsInChildren<Text>()) list.enabled = false;
        foreach (Image image in transform.GetComponentsInChildren<Image>()) if (image.name != "Portrait") image.enabled = false;
    }

    public void OnEndDrag(PointerEventData eventData) {
        transform.position = startPosition;
        GraphicRaycaster m_Raycaster = GetComponentInParent<GraphicRaycaster>();
        PointerEventData m_PointEventData = new PointerEventData(FindObjectOfType<EventSystem>());
        m_PointEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointEventData, results);

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
        deckShuffler.UseCard(gameObject);
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
