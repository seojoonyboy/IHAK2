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
    public int buildingMaxCount;
    public int setCount = 0;

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
                if (buildingMaxCount - deckSettingController.OnTileBuildingCount(setObject) > 0) {
                    if (targetTile.GetComponent<TileObject>().Tier <= AccountManager.Instance.userTier) {
                        SettingBuilding(setObject);
                        deckSettingController.picking = false;
                    }
                    else
                        return;
                }
                else
                    return;
            }
            else if (targetTile.GetComponent<TileObject>().buildingSet == true && targetTile != null) {
                CardBuildingSwap();
                setCount = 0;
                return;
            }
        }
        
        //RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, 5000);
        
    }

    public void SettingBuilding(GameObject building) {
        if (building == null) return;
        GameObject selectBuilding = Instantiate(building);
        int tileNum = targetTile.GetComponent<TileObject>().tileNum;
        transform.parent.parent.GetComponent<DeckSettingController>().tileSetList[tileNum] = building.GetComponent<BuildingObject>().data.id;
        Vector3 setLocation = targetTile.transform.position;
        setLocation.z = 0;
        BuildingObject buildingObject = selectBuilding.GetComponent<BuildingObject>();
        selectBuilding.transform.localPosition = setLocation;
        selectBuilding.transform.SetParent(targetTile.transform);
        selectBuilding.transform.localScale = new Vector3(1, 1, 1);
        if (selectBuilding.GetComponent<SpriteRenderer>() != null) {
            selectBuilding.GetComponent<SpriteRenderer>().sprite = buildingObject.mainSprite;
            selectBuilding.GetComponent<SpriteRenderer>().sortingOrder = (targetTile.transform.parent.childCount - 1) * 2 - targetTile.GetComponent<TileObject>().tileNum;
        }
        else {
            selectBuilding.GetComponent<MeshRenderer>().sortingOrder = (targetTile.transform.parent.childCount - 1) * 2 - targetTile.GetComponent<TileObject>().tileNum;
        }
        targetTile.GetComponent<TileObject>().buildingSet = true;
        GameObject card = deckSettingController.FindCard(selectBuilding.GetComponent<BuildingObject>().data.id);

        if (card != null) {
            //slot.transform.GetChild(2).GetComponent<UnityEngine.UI.Text>().text = deckSettingController.BuildingCount(slot.GetComponent<DragHandler>().setObject).ToString() + " / " + slot.GetComponent<DragHandler>().setObject.GetComponent<BuildingObject>().data.card.placementLimit.ToString();
            int count = buildingMaxCount - deckSettingController.OnTileBuildingCount(card.GetComponent<DragHandler>().setObject) - setCount;
            if (count <= 0) {
                card.GetComponent<Image>().color = Color.gray;
                card.transform.GetChild(0).GetComponent<Image>().color = Color.gray;
                card.transform.GetChild(1).GetComponent<Text>().color = Color.gray;
                card.transform.GetChild(2).GetComponent<Text>().color = Color.gray;
            }
            else {
                card.GetComponent<Image>().color = Color.white;
                card.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                card.transform.GetChild(1).GetComponent<Text>().color = Color.white;
                card.transform.GetChild(2).GetComponent<Text>().color = Color.white;
            }

            card.transform.GetChild(2).GetComponent<Text>().text = count.ToString() + " / " + buildingMaxCount.ToString();
        }
        deckSettingController.AddActiveSlot(selectBuilding);
        string prodType = buildingObject.data.card.prodType;
        Cost cost = buildingObject.data.card.product;

        deckSettingController.ChangeSliderValue(cost);
    }


    public void CardBuildingSwap() {   
        if (targetTile.transform.GetChild(0).GetComponent<BuildingObject>().data.id == -1)
            return;

        if (targetTile.transform.GetChild(0).GetComponent<BuildingObject>().data.id == setObject.GetComponent<BuildingObject>().data.id)
            return;

        GameObject targetTileBuilding = targetTile.transform.GetChild(0).gameObject;
        deckSettingController.DeleteBuilding(targetTileBuilding);
        setCount = 1;
        SettingBuilding(setObject);
    }


}
