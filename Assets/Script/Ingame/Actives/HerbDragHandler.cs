using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class HerbDragHandler : SpellCardDragHandler {
    void Start() {
        base.MoveBlock();
    }

    public override void OnEndDrag(PointerEventData eventData) {
        base.OnEndDrag(eventData);
        if (UseCard()) {
            GetComponent<HerbDragHandler>().enabled = false;

            obj.GetComponent<Herb>().StartHealing();

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

    public override void OnBeginDrag(PointerEventData eventData) {
        Setting();
        GetComponentInChildren<BoundaryCamMove>().isDrag = true;
        obj.GetComponent<Herb>().Init(data);
    }
}
