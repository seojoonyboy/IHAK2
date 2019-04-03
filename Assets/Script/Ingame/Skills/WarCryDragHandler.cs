using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WarCryDragHandler : IngameActiveCardDragHandler {
    public override void OnEndDrag(PointerEventData eventData) {
        UseCard();

        if (!PlayerController.Instance.deckShuffler().CanUseCard(GetComponent<ActiveCardInfo>())) return;

        var lists = FindObjectsOfType<UnitAI>();
        foreach(UnitAI unit in lists) {
            //아군 유닛만
            if(unit.gameObject.layer == 10 && (unit.GetComponent<MinionAI>() != null || unit.GetComponent<HeroAI>() != null)) {
                WarCry warCry = unit.gameObject.AddComponent<WarCry>();
                warCry.Init(data);
                warCry.StartBuff();
            }
        }

        ActiveCardCoolTime coolComp = parentBuilding.AddComponent<ActiveCardCoolTime>();
        coolComp.targetCard = gameObject;
        coolComp.coolTime = coolTime;
        coolComp.behaviour = this;
        coolComp.StartCool();
    }
}
