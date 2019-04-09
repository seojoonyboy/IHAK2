using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour, IDropHandler {
    
    public GameObject card {
        get {
            if (transform.childCount > 0) {
                return transform.GetChild(0).gameObject;
            }
            return null;
        }
    }
    
    public void OnDrop (PointerEventData eventData) {
        
        if(card == null) {
            try {
                SetCard(eventData.pointerPress.gameObject);
            }
            catch (NullReferenceException ne) {
                Debug.Log("카드선택오류");
            }
        }
        else if(card != null) {
            try {
                SwapCard(eventData.pointerPress.gameObject);
            }
            catch (NullReferenceException ne) {
                Debug.Log("카드선택오류");
            }
        }
        
    }

    public void SetCard(GameObject dragObject) {
        if (card != null) return;
        if (dragObject == null) return;
        if (transform.parent.parent.name != dragObject.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().card.data.type && transform.parent.parent.name != "wild") return;
        if (DeckSettingController.Instance.buildingCount >= 10) return;
        GameObject cardObject = Instantiate(DeckSettingController.Instance.originalCard, transform);
        cardObject.GetComponent<Image>().sprite = dragObject.GetComponent<Image>().sprite;
        cardObject.transform.Find("Image").GetComponent<Image>().sprite = dragObject.transform.Find("Data").GetComponent<Image>().sprite;
        cardObject.transform.Find("Mark").GetChild(0).GetComponent<Image>().sprite = dragObject.transform.Find("SecondMark").GetChild(0).GetComponent<Image>().sprite;


        DeckSettingController deckSettingController = DeckSettingController.Instance;
        int slotNum = transform.GetSiblingIndex();
        deckSettingController.buildingCount++;

        switch (transform.parent.parent.name) {
            case "hero":
                deckSettingController.heroList[slotNum] = dragObject.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().card.id;
                break;
            case "active":
                deckSettingController.activeList[slotNum] = dragObject.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().card.id;
                break;
            case "passive":
                deckSettingController.passiveList[slotNum] = dragObject.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().card.id;
                break;
            case "wild":
                deckSettingController.wildcard = dragObject.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().card.id;
                break;
        }
    }

    public void SwapCard(GameObject dragObject) {
        if (dragObject == null) return;
        if (transform.parent.parent.name != dragObject.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().card.data.type && transform.parent.parent.name != "wild") return;
        Destroy(card);
        GameObject cardObject = Instantiate(DeckSettingController.Instance.originalCard, transform);
        cardObject.GetComponent<Image>().sprite = dragObject.GetComponent<Image>().sprite;
        cardObject.transform.Find("Image").GetComponent<Image>().sprite = dragObject.transform.Find("Data").GetComponent<Image>().sprite;
        cardObject.transform.Find("Mark").GetChild(0).GetComponent<Image>().sprite = dragObject.transform.Find("SecondMark").GetChild(0).GetComponent<Image>().sprite;

        DeckSettingController deckSettingController = DeckSettingController.Instance;
        int slotNum = transform.GetSiblingIndex();
        deckSettingController.buildingCount++;

        switch (transform.parent.parent.name) {
            case "hero":
                deckSettingController.heroList[slotNum] = dragObject.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().card.id;
                break;
            case "active":
                deckSettingController.activeList[slotNum] = dragObject.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().card.id;
                break;
            case "passive":
                deckSettingController.passiveList[slotNum] = dragObject.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().card.id;
                break;
            case "wild":
                deckSettingController.wildcard = dragObject.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().card.id;
                break;
        }
    }
    


}
