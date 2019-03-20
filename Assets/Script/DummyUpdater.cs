using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyUpdater : MonoBehaviour {

    public GameObject group;
    ConstructManager constructManager;
    public GameObject constructs;

    private void Start() {
        constructManager = ConstructManager.Instance;
        constructs = constructManager.transform.gameObject;


        for(int i = 0; i< group.transform.childCount - 1; i++) {
            if(group.transform.GetChild(i).GetComponent<TileObject>().buildingSet == true) {
                GameObject building;
                for(int j = 0; j < constructs.transform.GetChild(0).childCount; j++) {
                    if(group.transform.GetChild(i).GetChild(0).GetComponent<BuildingObject>().card.id == constructs.transform.GetChild(0).GetChild(j).GetComponent<BuildingObject>().card.id) {
                        building = constructs.transform.GetChild(0).GetChild(j).gameObject;

                        Destroy(group.transform.GetChild(i).GetChild(0));
                        building = Instantiate(building, group.transform.GetChild(i));
                        building.transform.position = group.transform.GetChild(i).position;
                        building.GetComponent<BuildingObject>().setTileLocation = group.transform.GetChild(i).GetComponent<TileObject>().tileNum;
                        building.GetComponent<SpriteRenderer>().sprite = building.GetComponent<BuildingObject>().mainSprite;
                        building.GetComponent<SpriteRenderer>().sortingOrder = (group.transform.GetChild(i).childCount - 1) * 2 - group.transform.GetChild(i).GetComponent<TileObject>().tileNum;
                        break;
                    }
                }         
            }
        }
    }

}
