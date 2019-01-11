using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;

public class DropHandler : MonoBehaviour {

    public GameObject setObject;
    public GameObject targetTile;
    public DeckEditor deckEditor;
    public float startCamSize;
    public float camSize;
    Camera cam;

    private void Start() {
        cam = Camera.main;
        startCamSize = cam.orthographicSize;
        var camSizeStream = cam.ObserveEveryValueChanged(_ => cam.orthographicSize).Subscribe(_ => camSize = cam.orthographicSize);
    }

    public void OnDrop() {
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
            deckEditor.deckData[tileNum] = setObject.GetComponent<BuildingObject>().data.id;
            Vector3 setLocation = targetTile.transform.position;
            setLocation.z = 0;
            selectBuilding.transform.localPosition = setLocation;
            selectBuilding.transform.SetParent(targetTile.transform);
            selectBuilding.GetComponent<SpriteRenderer>().sortingOrder = targetTile.transform.parent.childCount - targetTile.GetComponent<TileObject>().tileNum;
            targetTile.GetComponent<TileObject>().buildingSet = true;
        }
        else
            return;

        //RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, 5000);
    }
}
