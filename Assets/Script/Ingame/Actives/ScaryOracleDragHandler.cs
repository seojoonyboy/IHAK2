using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class ScaryOracleDragHandler : SpellCardDragHandler {
    void Start() {
        //base.MoveBlock();
    }

    public override void OnEndDrag() {
        GetComponent<ScaryOracleDragHandler>().enabled = false;

        ScaryOracleEmiiter oracle = gameObject.AddComponent<ScaryOracleEmiiter>();
        oracle.Init(data);
        oracle.StartDebuff();

        ActiveCardCoolTime coolComp = parentBuilding.AddComponent<ActiveCardCoolTime>();
        coolComp.targetCard = GetComponent<SpellCardDragHandler>().targetCard;
        coolComp.coolTime = coolTime;
        coolComp.behaviour = this;
        coolComp.StartCool();

        PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = false;
        PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = true;
        //GetComponentInChildren<BoundaryCamMove>().isDrag = false;
    }

    public override void OnBeginDrag() {
        //Setting();
        //GetComponentInChildren<BoundaryCamMove>().isDrag = true;
        //obj.GetComponent<ScaryOracleEmiiter>().Init(data);
    }
}
