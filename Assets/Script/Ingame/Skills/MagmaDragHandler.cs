using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class MagmaDragHandler : IngameActiveCardDragHandler {
    public override void OnEndDrag(PointerEventData eventData) {
        if (UseCard()) {
            GetComponent<MagmaDragHandler>().enabled = false;

            obj.GetComponent<Magma>().StartDamaging();

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

        obj.GetComponent<Magma>().Init(data);
    }
}