using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;

public class TileManager : MonoBehaviour {

    [SerializeField]
    public GameObject tile;
    [SerializeField]
    public GameObject tileGroup;
    [SerializeField]
    public GameObject townCenter;
    [SerializeField]
    public Sprite[] tileSprite;

    void Start() {
        /*
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
    */
        CreateBackGroundTile();
    }


    public void CreateBackGroundTile() {
        Vector3 rowRange = tileGroup.transform.GetChild(5).position - tileGroup.transform.GetChild(0).position;
        Vector3 columnRange = tileGroup.transform.GetChild(1).position - tileGroup.transform.GetChild(0).position;

        GameObject backGround = tileGroup.transform.GetChild(tileGroup.transform.childCount - 1).gameObject;
        int[] outsideTile = new int[] { 0, 1, 2, 3, 4, 5, 9, 10, 14, 15, 19, 20, 21, 22, 23, 24 };
        Debug.Log(backGround.name);

        int columnCount = 6;
        int rowCount = 6;


        for(int i = 0; i<outsideTile.Length; i++) {
            GameObject tile = tileGroup.transform.GetChild(outsideTile[i]).gameObject;
            Vector3 tilePos = tile.transform.position;

            GameObject backTile;

            if(tile.GetComponent<TileObject>().location.row == 0) {
                if(tile.GetComponent<TileObject>().location.col == 0) {
                    for (int j = 1; j < 7; j++) {
                        backTile = Instantiate(tile, tilePos - columnRange*j - rowRange*j, Quaternion.identity);
                        backTile.transform.SetParent(backGround.transform);
                        backTile.GetComponent<SpriteRenderer>().sprite = tileSprite[UnityEngine.Random.Range(0, 4)];
                        
                        if(j >= 2) {
                            for (int k = 1; k < j; k++) {
                                backTile = Instantiate(tile, tilePos - columnRange * j + columnRange*k - rowRange * j, Quaternion.identity);
                                backTile.transform.SetParent(backGround.transform);
                                backTile.GetComponent<SpriteRenderer>().sprite = tileSprite[UnityEngine.Random.Range(0, 4)];

                                backTile = Instantiate(tile, tilePos - columnRange * j - rowRange * j + rowRange * k, Quaternion.identity);
                                backTile.transform.SetParent(backGround.transform);
                                backTile.GetComponent<SpriteRenderer>().sprite = tileSprite[UnityEngine.Random.Range(0, 4)];
                            }
                        }
                        
                    }
                }

                else if (tile.GetComponent<TileObject>().location.col == 4) {
                    backTile = Instantiate(tile, tilePos + columnRange - rowRange, Quaternion.identity);
                    backTile.transform.SetParent(backGround.transform);
                    backTile.GetComponent<SpriteRenderer>().sprite = tileSprite[UnityEngine.Random.Range(0, 4)];
                }

                
                for(int j = 1; j < columnCount; j++) {
                    backTile = Instantiate(tile, tilePos - (rowRange * j), Quaternion.identity);
                    backTile.transform.SetParent(backGround.transform);
                    backTile.GetComponent<SpriteRenderer>().sprite = tileSprite[UnityEngine.Random.Range(0, 4)];
                }

                if (tile.GetComponent<TileObject>().location.col < 2)
                    columnCount++;
                else if (tile.GetComponent<TileObject>().location.col >= 2)
                    columnCount--;
            }

            else if(tile.GetComponent<TileObject>().location.row == 4) {

                if (tile.GetComponent<TileObject>().location.col == 0) {
                    backTile = Instantiate(tile, tilePos - columnRange + rowRange, Quaternion.identity);
                    backTile.transform.SetParent(backGround.transform);
                    backTile.GetComponent<SpriteRenderer>().sprite = tileSprite[UnityEngine.Random.Range(0, 4)];
                }

                else if (tile.GetComponent<TileObject>().location.col == 4) {
                    for (int j = 1; j < 7; j++) {
                        backTile = Instantiate(tile, tilePos + columnRange*j + rowRange*j, Quaternion.identity);
                        backTile.transform.SetParent(backGround.transform);
                        backTile.GetComponent<SpriteRenderer>().sprite = tileSprite[UnityEngine.Random.Range(0, 4)];

                        if (j >= 2) {
                            for (int k = 1; k < j; k++) {
                                backTile = Instantiate(tile, tilePos + columnRange * j - columnRange * k + rowRange * j, Quaternion.identity);
                                backTile.transform.SetParent(backGround.transform);
                                backTile.GetComponent<SpriteRenderer>().sprite = tileSprite[UnityEngine.Random.Range(0, 4)];

                                backTile = Instantiate(tile, tilePos + columnRange * j + rowRange * j - rowRange * k, Quaternion.identity);
                                backTile.transform.SetParent(backGround.transform);
                                backTile.GetComponent<SpriteRenderer>().sprite = tileSprite[UnityEngine.Random.Range(0, 4)];
                            }
                        }
                    }
                }
                
                for(int j = 1; j < columnCount; j++) {
                    backTile = Instantiate(tile, tilePos + (rowRange * j), Quaternion.identity);
                    backTile.transform.SetParent(backGround.transform);
                    backTile.GetComponent<SpriteRenderer>().sprite = tileSprite[UnityEngine.Random.Range(0, 4)];
                }

                if (tile.GetComponent<TileObject>().location.col < 2)
                    columnCount++;
                else if (tile.GetComponent<TileObject>().location.col >= 2)
                    columnCount--;
                    
            }

            if (columnCount < 6)
                columnCount = 6;
            
            if(tile.GetComponent<TileObject>().location.col == 0) {
                for(int j = 1; j<rowCount; j++) {
                    backTile = Instantiate(tile, tilePos - (columnRange * j), Quaternion.identity);
                    backTile.transform.SetParent(backGround.transform);
                    backTile.GetComponent<SpriteRenderer>().sprite = tileSprite[UnityEngine.Random.Range(0, 4)];
                }
            }
            else if(tile.GetComponent<TileObject>().location.col == 4) {

                for(int j = 1; j< rowCount; j++) {
                    backTile = Instantiate(tile, tilePos + (columnRange * j), Quaternion.identity);
                    backTile.transform.SetParent(backGround.transform);
                    backTile.GetComponent<SpriteRenderer>().sprite = tileSprite[UnityEngine.Random.Range(0, 4)];
                }

                if (tile.GetComponent<TileObject>().location.row < 2)
                    rowCount++;
                else if (tile.GetComponent<TileObject>().location.row >= 2)
                    rowCount--;
            }

            if (rowCount < 6)
                rowCount = 6;

        }





    }


}
