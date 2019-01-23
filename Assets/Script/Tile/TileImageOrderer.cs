using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileImageOrderer : MonoBehaviour {
    private void OnEnable() {
        foreach (Transform tile in transform) {
            if (tile.childCount != 0) {
                tile.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = transform.childCount * 2 - tile.GetComponent<TileObject>().tileNum;
            }
        }
    }
}
