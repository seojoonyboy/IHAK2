using DataModules;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class HerbDragHandler : SpellCardDragHandler {

    public override void OnEndDrag(PointerEventData eventData) {
        base.OnEndDrag(eventData);

        //RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        //UI가 있는 곳에서 Drop시 스킬 발동 하지 않음
        GraphicRaycaster m_Raycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>(); ;
        PointerEventData m_PointEventData = new PointerEventData(FindObjectOfType<EventSystem>());
        m_PointEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointEventData, results);

        if (results.Count != 0) {
            obj.SetActive(false);
            return;
        }

        if (UseCard()) {
            obj.GetComponent<Herb>().StartHealing();

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

        obj.GetComponent<Herb>().Init(data);
    }
}
