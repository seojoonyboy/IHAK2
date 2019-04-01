using UnityEngine.EventSystems;

public class HerbDragHandler : IngameActiveCardDragHandler {
    public override void OnEndDrag(PointerEventData eventData) {
        obj.GetComponent<Herb>().StartHealing();

        ActiveCardCoolTime coolComp = parentBuilding.AddComponent<ActiveCardCoolTime>();
        coolComp.targetCard = gameObject;
        coolComp.coolTime = coolTime;
        coolComp.behaviour = this;
        coolComp.StartCool();

        UseCard();
    }
}
