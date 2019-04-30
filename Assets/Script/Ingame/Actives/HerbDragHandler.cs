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
        Herb herb = gameObject.AddComponent<Herb>();
        herb.Init(data);
        herb.StartHealing();

        ActiveCardCoolTime coolComp = targetCard.AddComponent<ActiveCardCoolTime>();
        coolComp.targetCard = GetComponent<SpellCardDragHandler>().targetCard;
        coolComp.coolTime = coolTime;
        coolComp.behaviour = this;
        coolComp.StartCool();
        GetComponent<SpellCardDragHandler>().enabled = false;
        //PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = false;
        //PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = true;

        //GetComponentInChildren<BoundaryCamMove>().isDrag = false;
    }

    public override void OnBeginDrag() {
        base.OnBeginDrag();
        //Setting();
        //GetComponentInChildren<BoundaryCamMove>().isDrag = true;
        //obj.GetComponent<Herb>().Init(data);
    }
}
