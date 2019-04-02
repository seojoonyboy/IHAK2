using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScaryOracleDragHandler : IngameActiveCardDragHandler {
    public override void OnEndDrag(PointerEventData eventData) {
        obj.GetComponent<ScaryOracle>().StartDebuff();

        ActiveCardCoolTime coolComp = parentBuilding.AddComponent<ActiveCardCoolTime>();
        coolComp.targetCard = gameObject;
        coolComp.coolTime = coolTime;
        coolComp.behaviour = this;
        coolComp.StartCool();

        UseCard();
    }

    public override void OnBeginDrag(PointerEventData eventData) {
        Setting();

        obj.GetComponent<ScaryOracle>().Init(data);
    }
}
