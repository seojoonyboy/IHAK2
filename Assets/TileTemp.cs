using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileTemp : MonoBehaviour {

    public Sprite[] tile;
    // Use this for initialization
    void Start() {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = tile[Random.Range(0, 4)];
    }	
}
