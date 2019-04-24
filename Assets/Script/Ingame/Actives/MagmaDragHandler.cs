using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class MagmaDragHandler : SpellCardDragHandler {
    void Start() {
        base.MoveBlock();
    }

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
        else {
            obj.SetActive(false);
        }
        PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = false;
        PlayerController.Instance.deckShuffler().spellCardParent.GetComponent<FlowLayoutGroup>().enabled = true;

        GetComponentInChildren<BoundaryCamMove>().isDrag = false;
    }

    public override void OnBeginDrag(PointerEventData eventData) {
        Setting();
        GetComponentInChildren<BoundaryCamMove>().isDrag = true;
        obj.GetComponent<Magma>().Init(data);
    }
}