using System.Collections;
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
        if (card == null) {
            SetCard(eventData.pointerDrag.gameObject);
        }
        else if( card != null) {
            SwapCard(eventData.pointerDrag.gameObject);
        }
    }

    public void SetCard(GameObject dragObject) {
        if (card != null) return;
        GameObject cardObject = Instantiate(DeckSettingController.Instance.originalCard, transform);
        cardObject.GetComponent<Image>().sprite = dragObject.GetComponent<Image>().sprite;
        cardObject.transform.Find("Image").GetComponent<Image>().sprite = dragObject.transform.Find("Data").GetComponent<Image>().sprite;
        cardObject.transform.Find("Mark").GetChild(0).GetComponent<Image>().sprite = dragObject.transform.Find("SecondMark").GetChild(0).GetComponent<Image>().sprite;
    }

    public void SwapCard(GameObject dragObject) {
        Destroy(card);
        GameObject cardObject = Instantiate(DeckSettingController.Instance.originalCard, transform);
        cardObject.GetComponent<Image>().sprite = dragObject.GetComponent<Image>().sprite;
        cardObject.transform.Find("Image").GetComponent<Image>().sprite = dragObject.transform.Find("Data").GetComponent<Image>().sprite;
        cardObject.transform.Find("Mark").GetChild(0).GetComponent<Image>().sprite = dragObject.transform.Find("SecondMark").GetChild(0).GetComponent<Image>().sprite;
    }


}
