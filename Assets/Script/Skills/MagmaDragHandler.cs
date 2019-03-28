using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class MagmaDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    IngameSceneEventHandler eventHandler;

    Vector3 startScale;
    Vector3 startPosition;

    [SerializeField] [ReadOnly] GameObject 
        magmaPref,
        magma;
    [SerializeField] [ReadOnly] Camera camera;
    [SerializeField] [ReadOnly] Transform parent;
    [SerializeField] [ReadOnly] bool isInit = false;
    [SerializeField] [ReadOnly] GameObject parentBuilding;
    [SerializeField] [ReadOnly] IngameDeckShuffler deckShuffler;

    public void Init(Camera camera, GameObject magma, Transform parent, GameObject parentBuilding, IngameDeckShuffler deckShuffler) {
        this.camera = camera;
        magmaPref = magma;
        this.parentBuilding = parentBuilding;
        this.deckShuffler = deckShuffler;
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

    private void BuildingDestroyed(Enum Event_Type, Component Sender, object Param) {
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
                GetComponent<MagmaDragHandler>().enabled = true;
            }
        }
    }

    public void CancelDrag() {
        GetComponent<MagmaDragHandler>().enabled = false;

        transform.position = startPosition;
        transform.localScale = new Vector3(1, 1, 1);

        GameObject deactive = transform.Find("Deactive").gameObject;
        deactive.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            PlayerController.Instance.deckShuffler()
            .cardParent
            .GetComponent<RectTransform>()
        );
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if(!isInit) {
            Debug.LogError("Magma의 초기화가 정상적으로 되지 않았습니다!");
            return;
        }

        if (magma == null) magma = Instantiate(magmaPref, parent);

        startPosition = transform.position;
        startScale = transform.localScale;
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;

        if (!isInit) {
            Debug.LogError("마그마 관련 초기화가 정상적으로 되지 않았습니다!");
            return;
        }

        magma.SetActive(true);
        Vector3 origin = camera.ScreenToWorldPoint(Input.mousePosition);
        magma.transform.position = new Vector3(origin.x, origin.y, 0);

    }

    public void OnEndDrag(PointerEventData eventData) {
        transform.position = startPosition;
        transform.localScale = new Vector3(1, 1, 1);

        magma.GetComponent<Magma>().StartDamaging();

        ActiveCardCoolTime coolComp = parentBuilding.AddComponent<ActiveCardCoolTime>();
        coolComp.targetCard = gameObject;
        coolComp.coolTime = 25;
        coolComp.behaviour = this;
        coolComp.StartCool();

        GetComponent<MagmaDragHandler>().enabled = false;

        deckShuffler.UseCard(gameObject);
    }
}