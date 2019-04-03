using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class HerbDragHandler : IngameActiveCardDragHandler {
    public override void OnEndDrag(PointerEventData eventData) {
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
    }

    public override void OnBeginDrag(PointerEventData eventData) {
        Setting();

        obj.GetComponent<Herb>().Init(data);
    }
}
