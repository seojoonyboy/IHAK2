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

    public void BeginDrag() {
        canDrag = true;
        OnBeginDrag(null);
    }

    private void Start() {
        dropHandler = GetComponentInParent<DropHandler>();
        Input.simulateMouseWithTouches = true;
        cam = Camera.main;
        startScale = transform.localScale;

        GetComponent<LongClickButton>().onShortClick.AddListener(() => Debug.Log("Short Button Click"));
    }

    public void OnBeginDrag (PointerEventData eventData) {
        if (!canDrag) return;
        dropHandler.setObject = setObject;
        startPosition = transform.position;        
        camMagnification = (dropHandler.startCamSize - dropHandler.camSize) * 0.025f;
        cam.GetComponent<BitBenderGames.MobileTouchCamera>().enabled = false;
    }

    public void OnEndDrag() {
        transform.localPosition = new Vector3(0, 14, 0);
        transform.localScale = startScale;
        cam.GetComponent<BitBenderGames.MobileTouchCamera>().enabled = true;

        Canvas.ForceUpdateCanvases();
        var glg = transform.parent.GetComponent<GridLayoutGroup>();
        glg.CalculateLayoutInputHorizontal();
        glg.CalculateLayoutInputVertical();
        glg.SetLayoutHorizontal();
        glg.SetLayoutVertical();
        canDrag = false;
        dropHandler.OnDrop();

        GetComponent<Image>().enabled = true;
        transform.Find("Name").GetComponent<Text>().enabled = true;
        transform.GetChild(2).GetComponent<Text>().enabled = true;    // slot => Count;
        transform.GetChild(0).GetComponent<Image>().color = Color.white;  //slot => Data;
    }

    public void OnDrag(PointerEventData eventData) {
        if (!canDrag) return;
        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null) {

            if(hit.collider.tag == "Tile") {
                GameObject tile = hit.transform.gameObject;
                transform.localScale = new Vector3(startScale.x * 1.5f + camMagnification, startScale.y * 1.5f + camMagnification);
                transform.position = cam.WorldToScreenPoint(tile.transform.position);

                GetComponent<Image>().enabled = false;
                transform.Find("Name").GetComponent<Text>().enabled = false;
                transform.GetChild(2).GetComponent<Text>().enabled = false;    // slot => Count;

                if (tile.GetComponent<TileObject>().buildingSet == false) {
                    if (AccountManager.Instance.userTier == tile.GetComponent<TileObject>().Tier)
                        transform.GetChild(0).GetComponent<Image>().color = Color.green;   //slot => Data;
                    else
                        transform.GetChild(0).GetComponent<Image>().color = Color.red;
                }
                else
                    transform.GetChild(0).GetComponent<Image>().color = Color.red;
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
