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
        detailModal;

    List<GameObject> buildings;

    private const int NUM_PER_PAGE = 14;
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
            setPage.transform.localPosition += new Vector3(i * Screen.width, 0);
        }
    }

    private void SetCards() {
        int page = 0;

        for (int i = 0; i < 16; i++) {
            if (i != 0 && i % NUM_PER_PAGE == 0)
                page++;

            int random = UnityEngine.Random.Range(0, 25);
            GameObject slotData = Instantiate(slotObject, transform.GetChild(page));
            GameObject buildingObject = slotData.GetComponentInChildren<DragHandler>().setObject = buildings[i];
            slotData.GetComponentInChildren<Image>().sprite = buildings[i].GetComponent<BuildingObject>().icon;

            slotData.GetComponentInChildren<LongClickButton>().onShortClick.AddListener(() => ShowDetail(buildingObject.GetComponent<BuildingObject>()));
        }
    }

    private void ShowDetail(BuildingObject buildingObject) {
        detailModal.SetActive(true);
        Transform innerModal = detailModal.transform.GetChild(0);

        Text hp = innerModal.Find("ImageArea/Image/HPArea/Value").GetComponent<Text>();
        Text header = innerModal.Find("Header/Text").GetComponent<Text>();
        Text limitCount = innerModal.Find("DataArea/LimitCount/Text").GetComponent<Text>();

        Card card = buildingObject.data.card;
        hp.text = card.hitPoint.ToString();
        header.text = card.name;
        limitCount.text = card.placementLimit.ToString();
    }
}
