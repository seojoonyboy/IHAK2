using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class HerbDragHandler : SpellCardDragHandler {
    void Start() {
        //base.MoveBlock();
    }

    public override void OnDrag() {
        base.OnDrag();
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
        coolComp.behaviour = this;
        coolComp.StartCool();
        GetComponent<SpellCardDragHandler>().enabled = false;
    }
}
