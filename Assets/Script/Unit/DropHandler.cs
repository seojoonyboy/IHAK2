using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Grids2D;

public class DropHandler : MonoBehaviour, IDropHandler {

    public GameObject setObject;
    public GameObject targetTile;
    Camera cam;
    Grid2D grid;
    public List<int> deckData;
    public GameObject tileGroup;

    private void Start() {
        cam = Camera.main;
        grid = Grid2D.instance;
        tileGroup = GameObject.FindGameObjectWithTag("TileGroup");

        for (int i = 0; i < tileGroup.transform.childCount; i++) {
            if (i != 12)
                deckData.Add(0);
            else
                deckData.Add(100);
        }
    }

    public void OnDrop(PointerEventData eventData) {        

        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
            targetTile = hit.transform.gameObject;
        else
            return;

        if (targetTile.GetComponent<TileObject>().buildingSet == false) {
            GameObject selectBuilding = Instantiate(setObject);
            int tileNum = targetTile.GetComponent<TileObject>().tileNum;
            deckData[tileNum] = setObject.GetComponent<BuildingObject>().buildingID;
            Vector3 setLocation = grid.CellGetPosition(tileNum);
            setLocation.z = 0;
            selectBuilding.transform.localPosition = setLocation;
            selectBuilding.transform.SetParent(targetTile.transform);
            targetTile.GetComponent<TileObject>().buildingSet = true;
        }
        else
            return;

        //RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, 5000);


    }
}
