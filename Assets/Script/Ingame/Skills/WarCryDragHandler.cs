using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class WarCryDragHandler : SpellCardDragHandler {
    void Start() {
        base.MoveBlock();
    }

    public override void OnEndDrag(PointerEventData eventData) {
        base.OnEndDrag(eventData);
        if (UseCard()) {
            GetComponent<WarCryDragHandler>().enabled = false;

            var lists = FindObjectsOfType<UnitAI>();
            foreach (UnitAI unit in lists) {
                //아군 유닛만
                if (unit.gameObject.layer == 10 && (unit.GetComponent<MinionAI>() != null || unit.GetComponent<HeroAI>() != null)) {
                    WarCry warCry = unit.gameObject.AddComponent<WarCry>();
                    warCry.Init(data);
                    warCry.StartBuff();

                    unit.AddBuff("war_cry", warCry);
                }
            }

            ActiveCardCoolTime coolComp = parentBuilding.AddComponent<ActiveCardCoolTime>();
            coolComp.targetCard = gameObject;
            coolComp.coolTime = coolTime;
            coolComp.behaviour = this;
            coolComp.StartCool();
        }

        PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = false;
        PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = true;

        GetComponentInChildren<BoundaryCamMove>().isDrag = false;
    }
}
