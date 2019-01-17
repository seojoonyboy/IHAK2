using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OwnCardGenerator : MonoBehaviour {
    private ConstructManager constructManager;
    public GameObject
        pageObject,
        slotObject,
        prodDetailModal,
        unitGenDetailModal;

    [Header("UI")]
    public Sprite[] cardPanels;

    List<GameObject> buildings;

    private const int NUM_PER_PAGE = 8;
    // Use this for initialization
    void Start () {
        constructManager = ConstructManager.Instance;
        buildings = constructManager.GetBuildingObjects();

        SetPage(buildings.Count, NUM_PER_PAGE);
        SetCards();
    }

    private void SetPage(int totalNum, int numPerPage) {
        for(int i=0; i<totalNum/numPerPage + 1; i++) {
            GameObject setPage = Instantiate(pageObject, transform);
            setPage.transform.localPosition += new Vector3(i * 1080f, 0);
        }
    }

    private void SetCards() {
        int page = 0;

        for (int i = 0; i < constructManager.GetBuildingObjects().Count; i++) {
            if (i != 0 && i % NUM_PER_PAGE == 0)
                page++;
            GameObject slotData = Instantiate(slotObject, transform.GetChild(page));
            GameObject buildingObject = slotData.GetComponentInChildren<DragHandler>().setObject = buildings[i];
            BuildingObject info = buildings[i].GetComponent<BuildingObject>();

            if (info.data.card.type == "prod")
                slotData.GetComponent<Image>().sprite = cardPanels[0];
            else if (info.data.card.type == "military")
                slotData.GetComponent<Image>().sprite = cardPanels[3];
            else if (info.data.card.type == "special")
                slotData.GetComponent<Image>().sprite = cardPanels[2];
            else
                slotData.GetComponent<Image>().sprite = cardPanels[1];

            slotData.transform.Find("Data").GetComponent<Image>().sprite = info.icon;
            slotData.transform.Find("Name").GetComponent<Text>().text = info.name;
            slotData.GetComponentInChildren<LongClickButton>().onShortClick.AddListener(() => ShowDetail(buildingObject.GetComponent<BuildingObject>()));
        }
    }

    private void ShowDetail(BuildingObject buildingObject) {
        if(buildingObject.data.card.unit == null) {
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

            tier.text = card.rareity + " 등급";
            food.text = card.product.food.ToString();
            gold.text = card.product.gold.ToString();
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
            Unit unit = card.unit;
            tier.text = unit.tearNeed + " 등급";
            header.text = card.name;

            unitName.text = "유닛생산 " + unit.name;
            needResources.text = "식량 : " + unit.cost.food + "\n"
                + "골드 : " + unit.cost.gold + "\n"
                + "환경 : " + unit.cost.environment + "\n";

            unitSpec.text = "체력 : " + unit.hitPoint
                + "공격력 : " + unit.power
                + "공격 속도 : " + unit.attackSpeed
                + "공격 범위 : " + unit.attackRange
                + "이동 속도 : " + unit.moveSpeed
                + "요구 레벨 : " + unit.tearNeed;
        }
    }
}
