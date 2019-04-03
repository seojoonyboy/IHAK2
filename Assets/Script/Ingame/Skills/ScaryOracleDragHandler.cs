using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class ScaryOracleDragHandler : IngameActiveCardDragHandler {
    public override void OnEndDrag(PointerEventData eventData) {
        if (UseCard()) {
            GetComponent<ScaryOracleDragHandler>().enabled = false;

            obj.GetComponent<ScaryOracle>().StartDebuff();

            ActiveCardCoolTime coolComp = parentBuilding.AddComponent<ActiveCardCoolTime>();
            coolComp.targetCard = gameObject;
            coolComp.coolTime = coolTime;
            coolComp.behaviour = this;
            coolComp.StartCool();
        }

        PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = false;
        PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = true;
    }

    public override void OnBeginDrag(PointerEventData eventData) {
        Setting();

        obj.GetComponent<ScaryOracle>().Init(data);
    }
}
