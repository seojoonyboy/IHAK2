using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OwnCardGenerator : MonoBehaviour {
    private ConstructManager constructManager;
    public DeckSettingController deckSettingController;
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
        deckSettingController = DeckSettingController.Instance;
        buildings = constructManager.GetBuildingObjects();
        

        SetPage();
        SetCards();
    }

    private void SetPage(int totalNum, int numPerPage) {

        int page = totalNum / numPerPage + 1;

        if (totalNum != 0 && totalNum % numPerPage == 0)
            page--;

        for (int i = 0; i < page; i++) {
            GameObject setPage = Instantiate(pageObject, transform);
            setPage.transform.localPosition += new Vector3(i * 1920f, 0);
        }

        EditScenePanel editScenePanel = transform.parent.GetComponent<EditScenePanel>();
        editScenePanel.maxPage = page;
        editScenePanel.SavePagePosition();
    }

    private void SetPage() {
        GameObject page = transform.GetChild(0).gameObject;
        RectTransform pageSize = page.GetComponent<RectTransform>();
        int cardCount = constructManager.GetBuildingObjects().Count;
        float cardYSize = page.GetComponent<GridLayoutGroup>().cellSize.y;
        float spaceYSize = page.GetComponent<GridLayoutGroup>().spacing.y;

        float pageYSize = (cardYSize * (cardCount / 4)) + (spaceYSize * (cardCount / 4));
        pageSize.sizeDelta = new Vector2(pageSize.sizeDelta.x, pageYSize);
    }

    private void SetCards() {
        int page = 0;
        int count = 0;
        for (int i = 0; i < constructManager.GetBuildingObjects().Count; i++) {
            /*
            if (i != 0 && i % NUM_PER_PAGE == 0) {
                page++;
                count = 0;
            }
            */
            GameObject slotData = Instantiate(slotObject, transform.GetChild(page));
            GameObject buildingObject = slotData.GetComponentInChildren<DragHandler>().setObject = buildings[i];
            BuildingObject info = buildings[i].GetComponent<BuildingObject>();
            deckSettingController.totalCard.Add(slotData);

            slotData.GetComponent<Image>().sprite = cardPanels[info.card.data.rarity - 1];

            slotData.transform.Find("FirstMark").GetComponent<Image>().sprite = markIcons[info.card.data.rarity - 1];
            slotData.transform.Find("SecondMark").GetComponent<Image>().sprite = markIcons[info.card.data.rarity - 1];
            slotData.transform.Find("Data").GetComponent<Image>().sprite = info.icon;
            slotData.transform.Find("Name").GetComponent<Text>().text = "Lv1 " + info.name;

            string _type = null;
            if (!string.IsNullOrEmpty(info.card.data.type)) {
                if (info.card.data.type == "prod") {
                    _type = "prod";
                }


                if (info.card.data.unit != null && !string.IsNullOrEmpty(info.card.data.unit.name)) {
                    Debug.Log(info.card.data.unit.name);
                    _type = "unit";
                }
                else if (info.card.data.activeSkills.Length != 0) {
                    Debug.Log(info.card.data.activeSkills[0].name);
                    _type = "spell";
                }
            }
          //count++;
            slotData.transform.Find("SecondMark/Image").GetComponent<Image>().sprite = GetIcon(_type);          
            slotData.transform.GetChild(2).GetComponent<Text>().text = deckSettingController.maxbuildCount + " / " + deckSettingController.maxbuildCount;
            slotData.GetComponent<LongClickButton>().requiredHoldTime = 0.3f;
            slotData.GetComponent<DragHandler>().parentPageObject = transform.GetChild(page).gameObject;
            slotData.GetComponent<DragHandler>().sibilingData = count;
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
