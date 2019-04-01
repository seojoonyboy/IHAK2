using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class MagmaDragHandler : IngameActiveCardDragHandler {
    public override void OnEndDrag(PointerEventData eventData) {
        obj.GetComponent<Magma>().StartDamaging();

        ActiveCardCoolTime coolComp = parentBuilding.AddComponent<ActiveCardCoolTime>();
        coolComp.targetCard = gameObject;
        coolComp.coolTime = 25;
        coolComp.behaviour = this;
        coolComp.StartCool();

        UseCard();
    }
}