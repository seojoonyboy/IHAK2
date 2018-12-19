using System.Collections;
using System.Collections.Generic;
using Grids2D;
using UnityEngine;

public class TileManager : MonoBehaviour {

    [SerializeField]
    public GameObject tile;
    [SerializeField]
    public GameObject tileGroup;

    void Start() {
        Grid2D grid = Grid2D.instance;

        Vector3 gridLocation = grid.transform.position;
        gridLocation.z = 0;
        GameObject GroupPosition = Instantiate(tileGroup, gridLocation, Quaternion.identity);

        for (int i = 0; i < grid.CellGetIndex(grid.rowCount, grid.columnCount) + 1; i++) {
            Vector3 tilePosition = grid.CellGetPosition(i);
            tilePosition.z = 0;
            GameObject temp = Instantiate(tile, tilePosition, Quaternion.identity);
            temp.transform.SetParent(GroupPosition.transform);
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
