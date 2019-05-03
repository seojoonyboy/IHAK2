using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class ScaryOracleDragHandler : SpellCardDragHandler {

    public override void OnEndDrag(PointerEventData eventData) {
        base.OnEndDrag(eventData);
        if (UseCard()) {
            obj.GetComponent<ScaryOracleEmiiter>().StartDebuff();

            ActiveCardCoolTime coolComp = targetCard.AddComponent<ActiveCardCoolTime>();
            coolComp.targetCard = gameObject;
            coolComp.coolTime = coolTime;
            coolComp.behaviour = this;
            coolComp.StartCool();
        }
        else {
            obj.SetActive(false);
        }
        PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = false;
        PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = true;
    }

    public override void OnBeginDrag(PointerEventData eventData) {
        base.OnBeginDrag(eventData);
        obj.GetComponent<ScaryOracleEmiiter>().Init(data);
    }
}
