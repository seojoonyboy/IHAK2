using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class MagmaDragHandler : SpellCardDragHandler {
    void Start() {
        //base.MoveBlock();
    }

    public override void OnEndDrag() {
        base.OnEndDrag();
    }

    public override void OnBeginDrag() {
        base.OnBeginDrag();
    }

    public override void SpellActivated() {
        GetComponent<Magma>().StartDamaging();

        ActiveCardCoolTime coolComp = targetCard.AddComponent<ActiveCardCoolTime>();
        coolComp.targetCard = GetComponent<SpellCardDragHandler>().targetCard;
        coolComp.coolTime = coolTime;

        ConditionSet expSet = PlayerController.Instance
            .MissionConditionsController()
            .conditions.Find(x => x.condition == Conditions.cooltime_fix);
        if (expSet != null) {
            coolComp.coolTime = expSet.args[0];
            //Debug.Log("마그마 카드 쿨타임 보정 : " + expSet.args[0]);
        }

        coolComp.behaviour = this;
        coolComp.StartCool();

        GetComponent<SpellCardDragHandler>().enabled = false;
    }
}