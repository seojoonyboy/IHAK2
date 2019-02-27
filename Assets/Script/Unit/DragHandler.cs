using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler {
    DropHandler dropHandler;
    public GameObject setObject;
    Vector3 startScale;
    Vector3 startPosition;
    float camMagnification;
    Camera cam;
    public bool canDrag = false;
    public DeckSettingController deckSettingController;
    public AccountManager accountManager;
    private int buildingMaxCount;

    public void BeginDrag() {
        canDrag = true;
        OnBeginDrag(null);
    }

    private void Start() {
        dropHandler = GetComponentInParent<DropHandler>();
        buildingMaxCount = setObject.GetComponent<BuildingObject>().data.card.placementLimit;
        Input.simulateMouseWithTouches = true;
        cam = Camera.main;
        startScale = transform.localScale;
        deckSettingController = DeckSettingController.Instance;
        GetComponent<LongClickButton>().onShortClick.AddListener(() => Debug.Log("Short Button Click"));
        accountManager = AccountManager.Instance;
    }

    public void OnBeginDrag (PointerEventData eventData) {
        if (buildingMaxCount - deckSettingController.OnTileBuildingCount(setObject) <= 0) return;
        if (!canDrag) return;
        dropHandler.setObject = setObject;
        dropHandler.buildingMaxCount = buildingMaxCount;
        startPosition = transform.position;        
        camMagnification = (dropHandler.startCamSize - dropHandler.camSize) * 0.025f;
        deckSettingController.picking = true;
        //cam.GetComponent<BitBenderGames.MobileTouchCamera>().enabled = false;
    }

    public void OnEndDrag() {
        transform.localPosition = new Vector3(0, 14, 0);
        transform.localScale = startScale;
        //cam.GetComponent<BitBenderGames.MobileTouchCamera>().enabled = true;

        Canvas.ForceUpdateCanvases();
        var glg = transform.parent.GetComponent<GridLayoutGroup>();
        glg.CalculateLayoutInputHorizontal();
        glg.CalculateLayoutInputVertical();
        glg.SetLayoutHorizontal();
        glg.SetLayoutVertical();
        canDrag = false;
        deckSettingController.picking = false;
        deckSettingController.clicktime = 0f;

        GetComponent<Image>().enabled = true;
        transform.Find("Name").GetComponent<Text>().enabled = true;
        transform.GetChild(2).GetComponent<Text>().enabled = true;    // slot => Count;
        
        if (buildingMaxCount - deckSettingController.OnTileBuildingCount(setObject) > 0) {
            transform.GetChild(0).GetComponent<Image>().color = Color.white;  //slot => Data;
            transform.GetChild(1).GetComponent<Text>().color = Color.white;
            transform.GetChild(2).GetComponent<Text>().color = Color.white;
        }
        else if (buildingMaxCount - deckSettingController.OnTileBuildingCount(setObject) <= 0) {
            transform.GetChild(0).GetComponent<Image>().color = Color.gray;
            transform.GetChild(1).GetComponent<Text>().color = Color.gray;
            transform.GetChild(2).GetComponent<Text>().color = Color.gray;
        }
        dropHandler.OnDrop();
    }

    public void OnDrag(PointerEventData eventData) {
        if (buildingMaxCount - deckSettingController.OnTileBuildingCount(setObject) <= 0) return;
        if (!canDrag) return;
        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null) {
            if(hit.collider.tag == "Tile") {
                GameObject tile = hit.transform.gameObject;
                if (accountManager.userTier >= tile.GetComponent<TileObject>().Tier) {

                    transform.localScale = new Vector3(startScale.x * 1.5f + camMagnification, startScale.y * 1.5f + camMagnification);
                    transform.position = cam.WorldToScreenPoint(tile.transform.position);

                    GetComponent<Image>().enabled = false;
                    transform.Find("Name").GetComponent<Text>().enabled = false;
                    transform.GetChild(2).GetComponent<Text>().enabled = false;    // slot => Count;

                    if (tile.GetComponent<TileObject>().buildingSet == false) {
                        if (AccountManager.Instance.userTier >= tile.GetComponent<TileObject>().Tier)
                            transform.GetChild(0).GetComponent<Image>().color = Color.green;   //slot => Data;
                        else
                            transform.GetChild(0).GetComponent<Image>().color = Color.red;
                    }
                    else
                        transform.GetChild(0).GetComponent<Image>().color = Color.red;
                }
                else {
                    transform.position = Input.mousePosition;
                    transform.GetChild(0).GetComponent<Image>().color = Color.white;
                    transform.localScale = startScale;
                }
            }
            else if (hit.collider.tag == "Building") {
                GameObject tile = hit.transform.parent.gameObject;
                transform.localScale = new Vector3(startScale.x * 1.5f + camMagnification, startScale.y * 1.5f + camMagnification);
                transform.position = cam.WorldToScreenPoint(tile.transform.position);

                GetComponent<Image>().enabled = false;
                transform.Find("Name").GetComponent<Text>().enabled = false;
                transform.GetChild(2).GetComponent<Text>().enabled = false;    // slot => Count;

                transform.GetChild(0).GetComponent<Image>().color = Color.red; //slot => Data;
            }
            else if (hit.collider.tag == "BackGroundTile") {
                transform.position = Input.mousePosition;
                transform.GetChild(0).GetComponent<Image>().color = Color.white;
                transform.localScale = startScale;
            }
        }
        else {
            transform.position = Input.mousePosition;
            transform.GetChild(0).GetComponent<Image>().color = Color.white;
            transform.localScale = startScale;
        }
    }
}
