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
    public Sprite[] cardPanels;

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

            slotData.transform.Find("Data").GetComponent<Image>().sprite = info.icon;
            slotData.transform.Find("Name").GetComponent<Text>().text = info.name;
            //slotData.transform.GetChild(2).GetComponent<Text>().text = 0 + " / " + buildings[i].GetComponent<BuildingObject>().data.card.placementLimit.ToString(); //슬롯데이터중, 건물의 갯수 표기;
            slotData.transform.GetChild(2).GetComponent<Text>().text = 1 + " / " + 1;
            slotData.GetComponent<LongClickButton>().requiredHoldTime = 0.3f;
            //slotData.GetComponentInChildren<LongClickButton>().onShortClick.AddListener(() => ShowDetail(buildingObject.GetComponent<BuildingObject>()));
        }
    }

    
}
