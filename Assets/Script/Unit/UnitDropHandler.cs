using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Grids2D;

public class UnitDropHandler : MonoBehaviour, IDropHandler {

    public GameObject unit;
    public GameObject targetTile;
    Camera cam;
    Grid2D grid;

    private void Start() {
        cam = Camera.main;
        grid = Grid2D.instance;

    }

    public void OnDrop(PointerEventData eventData) {

        /*
        float z = grid.transform.position.z;
        GameObject selectUnit = Instantiate(unit);
        Vector3 gridlocation = cam.ScreenToWorldPoint(Input.mousePosition);
        Cell temp = grid.CellGetAtPosition(gridlocation);
        Debug.Log(grid.CellGetIndex(temp));
        Vector3 location = grid.CellGetPosition(grid.CellGetIndex(temp));
        location.z = 0;

        selectUnit.transform.localPosition = location;
        unit = null;     
        */

        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
            targetTile = hit.transform.gameObject;
        else
            return;

        if (targetTile.GetComponent<TileObject>().buildingSet == false) {
            GameObject selectUnit = Instantiate(unit);
            Vector3 unitLocation = grid.CellGetPosition(targetTile.GetComponent<TileObject>().tileNum);
            unitLocation.z = 0;
            selectUnit.transform.localPosition = unitLocation;
            targetTile.GetComponent<TileObject>().buildingSet = true;
        }
        else
            return;

        //RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, 5000);


    }
}
