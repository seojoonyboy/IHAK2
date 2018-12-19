using System.Collections;
using System.Collections.Generic;
using Grids2D;
using UnityEngine;

public class TileManager : MonoBehaviour {

    [SerializeField]
    public GameObject tile;
    
	void Start () {
        Grid2D grid = Grid2D.instance;
        for(int i = 0; i < grid.CellGetIndex(grid.rowCount,grid.columnCount); i++) {
            Vector3 temp = grid.CellGetPosition(i);
            temp.z = 0;
            Instantiate(tile, temp, Quaternion.identity);
        }		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
