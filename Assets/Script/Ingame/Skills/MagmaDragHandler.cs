using UnityEngine;
using UnityEngine.EventSystems;

public class MagmaDragHandler : IngameActiveCardDragHandler {
    public override void OnEndDrag(PointerEventData eventData) {
        UseCard();

        if (!PlayerController.Instance.deckShuffler().CanUseCard(GetComponent<ActiveCardInfo>())) return;

        obj.GetComponent<Magma>().StartDamaging();

        ActiveCardCoolTime coolComp = parentBuilding.AddComponent<ActiveCardCoolTime>();
        coolComp.targetCard = gameObject;
        coolComp.coolTime = coolTime;
        coolComp.behaviour = this;
        coolComp.StartCool();
    }

    public override void OnBeginDrag(PointerEventData eventData) {
        Setting();

        obj.GetComponent<Magma>().Init(data);
    }
}