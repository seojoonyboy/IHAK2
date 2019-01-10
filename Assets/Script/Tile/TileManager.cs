using System.Collections;
using System.Collections.Generic;
using Grids2D;
using UnityEngine;
using DataModules;

public class TileManager : MonoBehaviour {

    [SerializeField]
    public GameObject tile;
    [SerializeField]
    public GameObject tileGroup;
    [SerializeField]
    public GameObject townCenter;
    

    void Start () {        
        Grid2D grid = Grid2D.instance;

        Vector3 gridLocation = grid.transform.position;
        gridLocation.z = 0;
        GameObject GroupPosition = Instantiate(tileGroup, gridLocation, Quaternion.identity);

        for (int i = 0; i < grid.CellGetIndex(grid.rowCount, grid.columnCount) + 1; i++) {
            Vector3 tilePosition = grid.CellGetPosition(i);
            tilePosition.z = 0;
            GameObject createTile = Instantiate(tile, tilePosition, Quaternion.identity);
            createTile.name = string.Format("Tile[{0},{1}]", grid.CellGetRow(i), grid.CellGetColumn(i));
            createTile.transform.SetParent(GroupPosition.transform);
            createTile.GetComponent<TileObject>().tileNum = i;
            createTile.GetComponent<TileObject>().row = grid.CellGetRow(i);
            createTile.GetComponent<TileObject>().col = grid.CellGetColumn(i);

            if (i == 12) {
                GameObject centerBuild = Instantiate(townCenter, tilePosition, Quaternion.identity);
                createTile.GetComponent<TileObject>().buildingSet = true;
                centerBuild.GetComponent<SpriteRenderer>().sortingOrder = grid.CellGetIndex(grid.rowCount, grid.columnCount) + 1 - i;
                centerBuild.transform.SetParent(createTile.transform);
            }
        }     
	}
}
