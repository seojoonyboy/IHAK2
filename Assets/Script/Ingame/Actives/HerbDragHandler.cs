using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class HerbDragHandler : SpellCardDragHandler {
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
        Herb herb = gameObject.AddComponent<Herb>();
        herb.Init(data);
        herb.StartHealing();

        ActiveCardCoolTime coolComp = targetCard.AddComponent<ActiveCardCoolTime>();
        coolComp.targetCard = GetComponent<SpellCardDragHandler>().targetCard;
        coolComp.coolTime = coolTime;

        ConditionSet expSet = PlayerController.Instance
            .MissionConditionsController()
            .conditions.Find(x => x.condition == Conditions.cooltime_fix);
        if (expSet != null) {
            coolComp.coolTime = expSet.args[0];
            //Debug.Log("Herb CoolTime Fix : " + expSet.args[0]);
        }

        coolComp.behaviour = this;
        coolComp.StartCool();
        GetComponent<SpellCardDragHandler>().enabled = false;

        IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.MISSION_EVENT.USE_MAGIC, null, null);
    }
}
