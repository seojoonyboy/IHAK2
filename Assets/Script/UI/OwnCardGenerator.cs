using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OwnCardGenerator : MonoBehaviour {
    private ConstructManager constructManager;
    public GameObject
        pageObject,
        slotObject;

    [Header("UI")]
    public Sprite[]
        cardPanels,
        markIcons,
        passiveSkillIcons;

    List<GameObject> buildings;

    private const int NUM_PER_PAGE = 8;
    // Use this for initialization
    void Start() {
        constructManager = ConstructManager.Instance;
        buildings = constructManager.GetBuildingObjects();
        

        SetPage(buildings.Count, NUM_PER_PAGE);
        SetCards();
    }

    private void SetPage(int totalNum, int numPerPage) {

        int page = totalNum / numPerPage + 1;

        if (totalNum != 0 && totalNum % numPerPage == 0)
            page--;

        for (int i = 0; i < page; i++) {
            GameObject setPage = Instantiate(pageObject, transform);
            setPage.transform.localPosition += new Vector3(i * 1080f, 0);
        }

        EditScenePanel editScenePanel = transform.parent.GetComponent<EditScenePanel>();
        editScenePanel.maxPage = page;
        editScenePanel.SavePagePosition();
    }

    private void SetCards() {
        int page = 0;

        for (int i = 0; i < constructManager.GetBuildingObjects().Count; i++) {
            if (i != 0 && i % NUM_PER_PAGE == 0)
                page++;
            GameObject slotData = Instantiate(slotObject, transform.GetChild(page));
            GameObject buildingObject = slotData.GetComponentInChildren<DragHandler>().setObject = buildings[i];
            BuildingObject info = buildings[i].GetComponent<BuildingObject>();

            slotData.GetComponent<Image>().sprite = cardPanels[info.data.card.rarity - 1];

            slotData.transform.Find("FirstMark").GetComponent<Image>().sprite = markIcons[info.data.card.rarity - 1];
            slotData.transform.Find("SecondMark").GetComponent<Image>().sprite = markIcons[info.data.card.rarity - 1];
            slotData.transform.Find("Data").GetComponent<Image>().sprite = info.icon;
            slotData.transform.Find("Name").GetComponent<Text>().text = "Lv1 " + info.name;

            string _type = null;
            if (!string.IsNullOrEmpty(info.data.card.type)) {
                if (info.data.card.type == "prod") {
                    _type = info.data.card.prodType;
                }

                if (info.data.card.unit != null && !string.IsNullOrEmpty(info.data.card.unit.name)) {
                    Debug.Log(info.data.card.unit.name);
                    _type = "unit";
                }
                else if (info.data.card.activeSkills.Length != 0) {
                    Debug.Log(info.data.card.activeSkills[0].name);
                    _type = "spell";
                }
            }
            slotData.transform.Find("SecondMark/Image").GetComponent<Image>().sprite = GetIcon(_type);
            //slotData.transform.GetChild(2).GetComponent<Text>().text = 0 + " / " + buildings[i].GetComponent<BuildingObject>().data.card.placementLimit.ToString(); //슬롯데이터중, 건물의 갯수 표기;
            slotData.transform.GetChild(2).GetComponent<Text>().text = 1 + " / " + 1;
            slotData.GetComponent<LongClickButton>().requiredHoldTime = 0.3f;
            //slotData.GetComponentInChildren<LongClickButton>().onShortClick.AddListener(() => ShowDetail(buildingObject.GetComponent<BuildingObject>()));
        }
    }

    private Sprite GetIcon(string keyword) {
        Sprite sprite = null;
        switch (keyword) {
            case "gold":
                sprite = passiveSkillIcons[3];
                break;
            case "food":
                sprite = passiveSkillIcons[2];
                break;
            case "env":
                sprite = passiveSkillIcons[1];
                break;
            case "unit":
                sprite = passiveSkillIcons[5];
                break;
            case "spell":
                sprite = passiveSkillIcons[4];
                break;
            default:
                sprite = passiveSkillIcons[6];
                break;
        }
        return sprite;
    }
}
