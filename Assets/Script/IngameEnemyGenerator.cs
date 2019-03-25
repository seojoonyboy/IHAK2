using Container;
using DataModules;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameEnemyGenerator : MonoBehaviour {
    [SerializeField] Transform tileGroupParent;
    [SerializeField] GameObject dummyPref;

    [SerializeField] [ReadOnly] TileGroup tileGroup;
    [SerializeField] [ReadOnly] PlayerResource playerResource;
    public GameObject goblin;

    /*
    public List<int> enemyBuildingList =  new List<int>{ 28, 36, 36, 49 ,47,
                                                         38, 44, 672, 29, 32,
                                                         40, 48, -1, 29, 32,
                                                         40, 48, 48, 30, 32,
                                                         47, 35, 35, 35, 28 };
                                                         */
    private void Awake() {
        //tileGroup = Instantiate(enemyTileGroup, enemyParent);
        //tileGroup.transform.Find("Background").gameObject.SetActive(false);
        //ingameCityManager.SetEnemyBuildingLists(ref tileGroup);
        //ingameCityManager.eachPlayersTileGroups.Add(tileGroup);
        playerResource = GetComponent<PlayerResource>();
    }

    public void Generate() {
        tileGroup = Instantiate(dummyPref, tileGroupParent)
            .transform
            .GetComponent<TileGroup>();

        tileGroup.transform.Find("Background").gameObject.SetActive(false);

        foreach (Transform tile in tileGroup.gameObject.transform) {
            if (tile.childCount == 1) {
                int tileNum = tile.GetComponent<TileObject>().tileNum;
                GameObject building = tile.GetChild(0).gameObject;
                BuildingObject buildingObject = building.GetComponent<BuildingObject>();
                CardData card = buildingObject.card.data;

                BuildingInfo info = new BuildingInfo(
                    tileNum: tileNum,
                    activate: true,
                    hp: card.hitPoint,
                    maxHp: card.hitPoint,
                    card: card,
                    gameObject: building
                );

                playerResource.TotalHp += info.maxHp;
                GetComponent<EnemyPlayerController>().buildingInfos.Add(info);
            }
        }
    }
}
