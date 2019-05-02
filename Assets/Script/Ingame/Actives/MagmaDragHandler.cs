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
        coolComp.behaviour = this;
        coolComp.StartCool();

        GetComponent<SpellCardDragHandler>().enabled = false;
    }
}