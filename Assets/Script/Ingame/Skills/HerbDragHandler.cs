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

    public override void OnBeginDrag(PointerEventData eventData) {
        Setting();

        obj.GetComponent<Herb>().Init(data);
    }
}
