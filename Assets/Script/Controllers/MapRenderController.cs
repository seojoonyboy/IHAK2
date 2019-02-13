using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapRenderController : MonoBehaviour {

    [SerializeField] Camera territoryCamera;
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            
            RaycastHit hit;
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition + territoryCamera.transform.position);
            if (Physics.Raycast(ray, out hit)) {
                Debug.Log(hit.point);
            }
        }
    }
}
