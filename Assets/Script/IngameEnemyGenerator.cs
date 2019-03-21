using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameEnemyGenerator : MonoBehaviour {
    public GameObject enemyTileGroup;
    public IngameCityManager ingameCityManager;
    public GameObject goblin;
    public Transform enemyParent;

    /*
    public List<int> enemyBuildingList =  new List<int>{ 28, 36, 36, 49 ,47,
                                                         38, 44, 672, 29, 32,
                                                         40, 48, -1, 29, 32,
                                                         40, 48, 48, 30, 32,
                                                         47, 35, 35, 35, 28 };
                                                         */

    public GameObject tileGroup;
    private void Awake() {
        tileGroup = Instantiate(enemyTileGroup, enemyParent);
        tileGroup.transform.Find("Background").gameObject.SetActive(false);
        ingameCityManager.SetEnemyBuildingLists(ref tileGroup);
        ingameCityManager.eachPlayersTileGroups.Add(tileGroup);
    }
}
