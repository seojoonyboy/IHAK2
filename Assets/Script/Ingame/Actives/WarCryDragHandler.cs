using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class WarCryDragHandler : SpellCardDragHandler {
    void Start() {
        //base.MoveBlock();
    }

    public override void OnEndDrag() {
        //base.OnEndDrag(eventData);
        //if (UseCard()) {
        //    GetComponent<WarCryDragHandler>().enabled = false;

        //    var lists = FindObjectsOfType<UnitAI>();
        //    foreach (UnitAI unit in lists) {
        //        //아군 유닛만
        //        if (unit.gameObject.layer == 10 && (unit.GetComponent<MinionAI>() != null || unit.GetComponent<HeroAI>() != null)) {
        //            WarCry warCry = unit.gameObject.AddComponent<WarCry>();
        //            warCry.Init(data);
        //            warCry.StartBuff();

        //            unit.AddBuff("war_cry", warCry);
        //        }
        //    }

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

    public override void Init(Camera camera, GameObject parentBuilding, IngameDeckShuffler deckShuffler, string[] data, int coolTime, GameObject targetCard) {
        this.camera = camera;
        this.parentBuilding = parentBuilding;
        this.deckShuffler = deckShuffler;
        this.data = data;
        this.coolTime = coolTime;
        this.targetCard = targetCard;
        isInit = true;

        int range = 1000;

        GetComponent<CircleCollider2D>().radius = range;
        transform.GetChild(0).localScale *= range;
    }
}
