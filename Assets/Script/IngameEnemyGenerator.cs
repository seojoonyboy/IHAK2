using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameEnemyGenerator : MonoBehaviour {
    public GameObject enemyTileGroup;
    public IngameCityManager ingameCityManager;

    /*
    public List<int> enemyBuildingList =  new List<int>{ 28, 36, 36, 49 ,47,
                                                         38, 44, 672, 29, 32,
                                                         40, 48, -1, 29, 32,
                                                         40, 48, 48, 30, 32,
                                                         47, 35, 35, 35, 28 };
                                                         */

    private void Start() {
        GameObject tileGroup = Instantiate(enemyTileGroup, transform);
        ingameCityManager.SetEnemyBuildingLists(ref tileGroup);
    }
}
