using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public GameObject setObject;
    Vector3 startScale;
    Vector3 startPosition;
    Vector3 picturePosition;
    float camMagnification;
    Camera cam;
    public bool canDrag = false;
    public bool onDeck = false;
    public DeckSettingController deckSettingController;

    [Header(" - PageSave")]
    public GameObject parentPageObject;
    public int sibilingData;


    private void Start() {
        canDrag = true;
        Input.simulateMouseWithTouches = true;
        cam = Camera.main;
        startScale = transform.localScale;
        deckSettingController = DeckSettingController.Instance;
        startPosition = transform.position;
    }

    public void OnBeginDrag (PointerEventData eventData) {
        if (!canDrag) return;
        picturePosition = transform.Find("Data").localPosition;
        transform.gameObject.GetComponent<Image>().raycastTarget = false;
        
        deckSettingController.picking = true;
    }

    public void OnEndDrag(PointerEventData eventData) {
        transform.localPosition = new Vector3(0, 0, 0);
        transform.localScale = startScale;
        
        transform.gameObject.GetComponent<Image>().raycastTarget = true;
        Canvas.ForceUpdateCanvases();

        if(onDeck == false) {
            var glg = transform.parent.GetComponent<GridLayoutGroup>();
            glg.CalculateLayoutInputHorizontal();
            glg.CalculateLayoutInputVertical();
            glg.SetLayoutHorizontal();
            glg.SetLayoutVertical();
        }

        if(onDeck == true) {
            DragDestroy(eventData.pointerEnter.gameObject);
        }

        deckSettingController.picking = false;
        deckSettingController.clicktime = 0f;  


    }

    public void OnDrag(PointerEventData eventData) {
        if (!canDrag) return;        
        transform.gameObject.GetComponent<Image>().raycastTarget = false;
        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        transform.position = Input.mousePosition;
        transform.GetChild(0).GetComponent<Image>().color = Color.white;
        transform.localScale = startScale;
    }

    public void DisableCard() {
        canDrag = false;
        transform.GetComponent<Image>().color = Color.gray;
        transform.Find("Data").GetComponent<Image>().color = Color.gray;
        transform.Find("Name").GetComponent<Text>().color = Color.gray;
        transform.Find("FirstMark").GetComponent<Image>().color = Color.gray;
        //transform.Find("FirstMark").Find("Text").GetComponent<Text>().color = Color.gray;
        transform.Find("SecondMark").GetComponent<Image>().color = Color.gray;
        transform.Find("SecondMark").Find("Image").GetComponent<Image>().color = Color.gray;
    }

    public void ActivateCard() {
        canDrag = true;
        transform.GetComponent<Image>().color = Color.white;
        transform.Find("Data").GetComponent<Image>().color = Color.white;
        transform.Find("Name").GetComponent<Text>().color = Color.white;
        transform.Find("FirstMark").GetComponent<Image>().color = Color.white;
        //transform.Find("FirstMark").Find("Text").GetComponent<Text>().color = Color.white;
        transform.Find("SecondMark").GetComponent<Image>().color = Color.white;
        transform.Find("SecondMark").Find("Image").GetComponent<Image>().color = Color.white;
    }

    public void DragDestroy(GameObject target) {
        if (onDeck == false) return;
        if (target != transform.parent.gameObject) {
            string type = transform.parent.parent.parent.name;

            switch (type) {
                case "hero":
                    deckSettingController.FindCard(deckSettingController.heroList[transform.parent.GetSiblingIndex()]).GetComponent<DragHandler>().canDrag = true;
                    deckSettingController.heroList[transform.parent.GetSiblingIndex()] = -1;
                    break;
                case "active":
                    deckSettingController.FindCard(deckSettingController.activeList[transform.parent.GetSiblingIndex()]).GetComponent<DragHandler>().canDrag = true;
                    deckSettingController.activeList[transform.parent.GetSiblingIndex()] = -1;
                    break;
                case "passive":
                    deckSettingController.FindCard(deckSettingController.passiveList[transform.parent.GetSiblingIndex()]).GetComponent<DragHandler>().canDrag = true;
                    deckSettingController.passiveList[transform.parent.GetSiblingIndex()] = -1;
                    break;
                case "wild":
                    deckSettingController.FindCard(deckSettingController.wildcard).GetComponent<DragHandler>().canDrag = true;
                    deckSettingController.wildcard = -1;
                    break;
            }
            deckSettingController.cardCount--;
            deckSettingController.SetDeckInfo();
            Destroy(gameObject);
        }

    }
}
