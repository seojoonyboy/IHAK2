using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OwnCardGenerator : MonoBehaviour {

    public GameObject pageObject;
    public GameObject slotObject;
    public GameObject cardData;

    // Use this for initialization
    void Start () {
        cardData = ConstructManager.Instance.gameObject;
        for (int i = 0; i < 16 / 14 + 1; i++) {
            GameObject setPage = Instantiate(pageObject, transform);
            setPage.transform.localPosition += new Vector3(i * Screen.width, 0);
        }

        int page = 0;

        for(int i = 0; i < 16; i++) {            
            if (i != 0 && i % 14 == 0)
                page++;

            int random = UnityEngine.Random.Range(0, 25);            
            GameObject slotData = Instantiate(slotObject, transform.GetChild(page));            
            GameObject setSlot = cardData.transform.GetChild(0).GetChild(random).gameObject;
            slotData.GetComponentInChildren<DragHandler>().setObject = setSlot;
            slotData.GetComponentInChildren<Image>().sprite = setSlot.GetComponent<BuildingObject>().icon;            
        }
    }
}
