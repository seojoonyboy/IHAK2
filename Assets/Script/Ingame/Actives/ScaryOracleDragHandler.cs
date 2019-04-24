using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class ScaryOracleDragHandler : SpellCardDragHandler {
    void Start() {
        //base.MoveBlock();
    }

    public override void OnEndDrag() {
        //base.OnEndDrag(eventData);
        //if (UseCard()) {
        //    GetComponent<ScaryOracleDragHandler>().enabled = false;

        //    obj.GetComponent<ScaryOracleEmiiter>().StartDebuff();

        //    ActiveCardCoolTime coolComp = parentBuilding.AddComponent<ActiveCardCoolTime>();
        //    coolComp.targetCard = gameObject;
        //    coolComp.coolTime = coolTime;
        //    coolComp.behaviour = this;
        //    coolComp.StartCool();
        //}

        //PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = false;
        //PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = true;
        //GetComponentInChildren<BoundaryCamMove>().isDrag = false;
    }

    public override void OnBeginDrag() {
        //Setting();
        //GetComponentInChildren<BoundaryCamMove>().isDrag = true;
        //obj.GetComponent<ScaryOracleEmiiter>().Init(data);
    }
}
