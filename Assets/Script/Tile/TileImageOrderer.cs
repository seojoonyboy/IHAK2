using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileImageOrderer : MonoBehaviour {
    private void OnEnable() {
        foreach (Transform tile in transform) {
            if (tile.name != "Background" && tile.childCount != 0) {
                if(tile.GetChild(0).GetComponent<SpriteRenderer>() == null)
                    tile.GetChild(0).GetComponent<MeshRenderer>().sortingOrder = (transform.childCount - 1) * 2 - tile.GetComponent<TileObject>().tileNum;
                else
                    tile.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = (transform.childCount - 1) * 2 - tile.GetComponent<TileObject>().tileNum;
            }
        }
    }
}
