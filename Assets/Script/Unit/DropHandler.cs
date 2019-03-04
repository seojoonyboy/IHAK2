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

    [Header (" - Status" )]
    [SerializeField] public GameObject prodDetailModal;
    [SerializeField] public GameObject unitGenDetailModal;

    private void Start() {
        cam = Camera.main;
        startCamSize = cam.orthographicSize;
        var camSizeStream = cam.ObserveEveryValueChanged(_ => cam.orthographicSize).Subscribe(_ => camSize = cam.orthographicSize);
        prodDetailModal = deckSettingController.prodDetailModal;
        unitGenDetailModal = deckSettingController.unitGenDetailModal;
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


    public void ShowDetail(BuildingObject buildingObject) {
        if (buildingObject == null) return;

        if (buildingObject.data.card.unit == null || string.IsNullOrEmpty(buildingObject.data.card.unit.name)) {
            prodDetailModal.SetActive(true);
            Transform innerModal = prodDetailModal.transform.GetChild(0);

            Text hp = innerModal.Find("DataArea/UpperBody/HP/Value").GetComponent<Text>();
            Text header = innerModal.Find("Header/Text").GetComponent<Text>();
            Text limitCount = innerModal.Find("Upper/LimitCount/Value").GetComponent<Text>();
            Text tier = innerModal.Find("Upper/DataArea/Header/TierName").GetComponent<Text>();

            Text food = innerModal.Find("DataArea/UpperBody/Food/Value").GetComponent<Text>();
            Text env = innerModal.Find("DataArea/UpperBody/Env/Value").GetComponent<Text>();
            Text gold = innerModal.Find("DataArea/UpperBody/Gold/Value").GetComponent<Text>();

            Card card = buildingObject.data.card;
            hp.text = card.hitPoint.ToString();
            header.text = card.name;
            limitCount.text = "한도 " + card.placementLimit.ToString();

            tier.text = card.rarity + " 등급";
            food.text = card.product.food.ToString();
            gold.text = card.product.gold.ToString();
            env.text = card.product.environment.ToString();

            Image image = innerModal.Find("Upper/ImageArea/Image").GetComponent<Image>();
            image.sprite = ConstructManager.Instance.GetComponent<BuildingImages>().GetImage(buildingObject.data.card.race, buildingObject.data.card.type, buildingObject.data.card.id);
            prodDetailModal.transform.GetChild(0).GetChild(4).gameObject.SetActive(false);
        }
        else {
            unitGenDetailModal.SetActive(true);
            Transform innerModal = unitGenDetailModal.transform.GetChild(0);

            Text tier = innerModal.Find("Upper/DataArea/Header/TierName").GetComponent<Text>();
            Text header = innerModal.Find("Header/Text").GetComponent<Text>();

            Text unitName = innerModal.Find("DataArea/UpperBody/Text").GetComponent<Text>();
            Text needResources = innerModal.Find("DataArea/UpperBody/NeedResource").GetComponent<Text>();
            Text unitSpec = innerModal.Find("DataArea/BottomBody/UnitSpec").GetComponent<Text>();

            Card card = buildingObject.data.card;
            DataModules.Unit unit = card.unit;

            tier.text = unit.tierNeed + " 등급";
            header.text = card.name;

            unitName.text = "유닛생산 " + unit.name;
            Debug.Log(tier.text);
            needResources.text = "식량 : " + unit.cost.food + "\n"
                + "골드 : " + unit.cost.gold + "\n"
                + "환경 : " + unit.cost.environment + "\n";

            unitSpec.text = "체력 : " + unit.hitPoint + "\n"
                + "공격력 : " + unit.power + "\n"
                + "공격 속도 : " + unit.attackSpeed + "\n"
                + "공격 범위 : " + unit.attackRange + "\n"
                + "이동 속도 : " + unit.moveSpeed + "\n"
                + "요구 레벨 : " + unit.tierNeed;

            Image image = innerModal.Find("Upper/ImageArea/Image").GetComponent<Image>();
            image.sprite = ConstructManager.Instance.GetComponent<BuildingImages>().GetImage(buildingObject.data.card.race, buildingObject.data.card.type, buildingObject.data.card.id);
            unitGenDetailModal.transform.GetChild(0).GetChild(4).gameObject.SetActive(false);
        }
        Debug.Log(prodDetailModal.transform.GetChild(0).GetChild(4).name);
    }


}
