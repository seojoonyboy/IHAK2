using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DataModules;

public class DropHandler : MonoBehaviour {

    public GameObject setObject;
    public GameObject targetTile;
    public float startCamSize;
    public float camSize;
    Camera cam;
    public DeckSettingController deckSettingController;

    private void Start() {
        cam = Camera.main;
        startCamSize = cam.orthographicSize;
        var camSizeStream = cam.ObserveEveryValueChanged(_ => cam.orthographicSize).Subscribe(_ => camSize = cam.orthographicSize);
    }

    public void OnDrop() {
        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null) {
            if (hit.collider.tag == "Tile")
                targetTile = hit.transform.gameObject;
            else if (hit.collider.tag == "Building")
                targetTile = hit.transform.parent.gameObject;
            else if (hit.collider.tag == "BackGroundTile")
                return; 

            if (targetTile.GetComponent<TileObject>().buildingSet == false && targetTile != null) {
                //if (setObject.GetComponent<BuildingObject>().data.card.placementLimit - deckSettingController.BuildingCount(setObject) > 0) {
                if (1 - deckSettingController.BuildingCount(setObject) > 0) {
                    if (targetTile.GetComponent<TileObject>().Tier == AccountManager.Instance.userTier) {
                        GameObject selectBuilding = Instantiate(setObject);
                        int tileNum = targetTile.GetComponent<TileObject>().tileNum;
                        transform.parent.parent.GetComponent<DeckSettingController>().tileSetList[tileNum] = setObject.GetComponent<BuildingObject>().data.id;
                        Vector3 setLocation = targetTile.transform.position;
                        setLocation.z = 0;
                        BuildingObject buildingObject = selectBuilding.GetComponent<BuildingObject>();
                        selectBuilding.GetComponent<SpriteRenderer>().sprite = buildingObject.mainSprite;
                        selectBuilding.transform.localPosition = setLocation;
                        selectBuilding.transform.SetParent(targetTile.transform);
                        selectBuilding.transform.localScale = new Vector3(1, 1, 1);
                        selectBuilding.GetComponent<SpriteRenderer>().sortingOrder = targetTile.transform.parent.childCount * 2 - targetTile.GetComponent<TileObject>().tileNum;
                        targetTile.GetComponent<TileObject>().buildingSet = true;
                        GameObject slot = deckSettingController.FindCard(selectBuilding.GetComponent<BuildingObject>().data.id);

                        if (slot != null) {
                            //slot.transform.GetChild(2).GetComponent<UnityEngine.UI.Text>().text = deckSettingController.BuildingCount(slot.GetComponent<DragHandler>().setObject).ToString() + " / " + slot.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().data.card.placementLimit.ToString();
                            int count = 1 - deckSettingController.BuildingCount(slot.GetComponent<DragHandler>().setObject);
                            if (count == 0) {                                                                
                                slot.GetComponent<Image>().color = Color.gray;
                                slot.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                            }
                            slot.transform.GetChild(2).GetComponent<Text>().text = count.ToString() + " / " + 1;
                        }
                        deckSettingController.AddActiveSlot(selectBuilding);
                        string prodType = buildingObject.data.card.prodType;
                        Cost cost = buildingObject.data.card.product;

                        deckSettingController.ChangeSliderValue(cost);
                    }

                    else
                        return;
                }
                else
                    return;
            }
            else
                return;
        }
        //RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, 5000);
    }
}
