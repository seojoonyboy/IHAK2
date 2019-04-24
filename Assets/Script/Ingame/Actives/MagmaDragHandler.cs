using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class MagmaDragHandler : SpellCardDragHandler {
    void Start() {
        //base.MoveBlock();
    }

    public override void OnEndDrag() {

        //if (UseCard()) {
        //    GetComponent<MagmaDragHandler>().enabled = false;

        GetComponent<Magma>().StartDamaging();

        ActiveCardCoolTime coolComp = targetCard.AddComponent<ActiveCardCoolTime>();
        coolComp.targetCard = GetComponent<SpellCardDragHandler>().targetCard;
        coolComp.coolTime = coolTime;
        coolComp.behaviour = this;
        coolComp.StartCool();

        GetComponent<SpellCardDragHandler>().enabled = false;
        //}
        //else {
        //    obj.SetActive(false);
        //}
        //PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = false;
        //PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = true;

        //GetComponentInChildren<BoundaryCamMove>().isDrag = false;
    }

    public override void OnBeginDrag() {
        //Setting();
        //GetComponentInChildren<BoundaryCamMove>().isDrag = true;
        //obj.GetComponent<Magma>().Init(data);
    }
}