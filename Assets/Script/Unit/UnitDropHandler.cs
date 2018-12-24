using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitDropHandler : MonoBehaviour, IDropHandler {

    public GameObject Unit;
    Camera cam;

    private void Start() {
        cam = Camera.main;
    }

    public void OnDrop(PointerEventData eventData) {

        GameObject Temp = Instantiate(Unit);
        Vector3 location = cam.ScreenToWorldPoint(Input.mousePosition);
        location.z = 0;
        Temp.transform.localPosition = location;
        
        
        

        Debug.Log("메롱!");

    }
}
