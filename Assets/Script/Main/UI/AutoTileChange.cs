using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTileChange : MonoBehaviour {
    LineRenderer lr;
    Material material;
    IEnumerator coroutine;

    void Start() {
        lr = GetComponent<LineRenderer>();
        material = lr.materials[0];

        coroutine = Tiling();
        StartCoroutine(coroutine);
    }

    IEnumerator Tiling() {
        material.SetTextureScale("_MainTex", new Vector2(1, 0));
        yield return new WaitForSeconds(1.0f);
    }

    void OnDestroy() {
        StopCoroutine(coroutine);
    }
}
