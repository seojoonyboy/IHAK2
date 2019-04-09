using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IngameActiveCardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    protected IngameSceneEventHandler eventHandler;

    protected Vector3 startScale;
    protected Vector3 startPosition;

    [SerializeField] protected GameObject prefab,obj;
    [SerializeField] protected Camera camera;
    [SerializeField] protected Transform parent;
    [SerializeField] protected bool isInit = false;
    [SerializeField] protected GameObject parentBuilding;
    [SerializeField] protected IngameDeckShuffler deckShuffler;
    [SerializeField] protected string[] data;
    [SerializeField] protected int coolTime;

    public void Init(Camera camera, GameObject prefab, Transform parent, GameObject parentBuilding, IngameDeckShuffler deckShuffler, string[] data, int coolTime) {
        this.camera = camera;
        this.prefab = prefab;
        this.parentBuilding = parentBuilding;
        this.deckShuffler = deckShuffler;
        this.data = data;
        this.coolTime = coolTime;
        isInit = true;
    }

    void Awake() {
        eventHandler = IngameSceneEventHandler.Instance;
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.BUILDING_DESTROYED, BuildingDestroyed);
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.BUILDING_RECONSTRUCTED, BuildingReconstucted);
    }

    void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.BUILDING_DESTROYED, BuildingDestroyed);
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.BUILDING_RECONSTRUCTED, BuildingReconstucted);
    }

    protected void BuildingDestroyed(Enum Event_Type, Component Sender, object Param) {
        IngameSceneEventHandler.BuildingDestroyedPackage parms = (IngameSceneEventHandler.BuildingDestroyedPackage)Param;
        if (parms.target == IngameHpSystem.Target.ME) {
            if (GetComponent<ActiveCardInfo>().data.parentBuilding == parms.buildingInfo.gameObject) {
                CancelDrag();
            }
        }
    }

    private void BuildingReconstucted(Enum Event_Type, Component Sender, object Param) {
        IngameSceneEventHandler.BuildingDestroyedPackage parms = (IngameSceneEventHandler.BuildingDestroyedPackage)Param;
        if (parms.target == IngameHpSystem.Target.ME) {
            if (GetComponent<ActiveCardInfo>().data.parentBuilding == parms.buildingInfo.gameObject) {
                GetComponent<IngameActiveCardDragHandler>().enabled = true;
            }
        }
    }

    private void CancelDrag() {
        GetComponent<MagmaDragHandler>().enabled = false;

        transform.position = startPosition;
        transform.localScale = new Vector3(1, 1, 1);

        GameObject deactive = transform.Find("Deactive").gameObject;
        deactive.SetActive(true);
    }

    public virtual void OnBeginDrag(PointerEventData eventData) { DragOn(); }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;

        if (!isInit) {
            Debug.LogError("Prefab 관련 초기화가 정상적으로 되지 않았습니다!");
            return;
        }

        if(obj != null) {
            obj.SetActive(true);
            Vector3 origin = camera.ScreenToWorldPoint(Input.mousePosition);
            obj.transform.position = new Vector3(origin.x, origin.y, 0);
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData) { }

    public void Setting() {
        if (!isInit) {
            Debug.LogError("Prefab의 초기화가 정상적으로 되지 않았습니다!");
            return;
        }

        if (obj == null) obj = Instantiate(prefab, parent);

        startPosition = transform.position;
        startScale = transform.localScale;
    }

    public bool UseCard() {
        transform.position = startPosition;
        transform.localScale = new Vector3(1, 1, 1);

        var result = deckShuffler.UseCard(gameObject);
        return result;
    }

    public void DragOn() {
        GetComponentInChildren<BoundaryCamMove>().isDrag = true;
    }

    public void DragOff() {
        GetComponentInChildren<BoundaryCamMove>().isDrag = false;
    }
}
