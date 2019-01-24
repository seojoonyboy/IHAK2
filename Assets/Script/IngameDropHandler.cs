using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;

public class IngameDropHandler : MonoBehaviour {
    public GameObject selectedObject;
    public GameObject[] unitPrefs;

    [SerializeField] IngameCityManager ingameCityManager;
    [SerializeField] IngameDeckShuffler ingameDeckShuffler;

    Camera cam;
    // Use this for initialization
    void Start() {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnDrop() {
        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if(hit.collider != null) {
            Debug.Log(hit.collider.tag);
            if(hit.collider.tag == "BackGroundTile") {
                if (ingameCityManager.CurrentView == 0) {
                    Debug.Log("아군 도시에는 유닛생산 불가");
                    return;
                }
                //Debug.Log(hit.collider.name);
                var tmp = ingameCityManager.eachPlayersTileGroups;
                
                IngameCard card = selectedObject.GetComponent<IngameCard>();
                object data = card.data;
                if(data.GetType() == typeof(Unit)) {
                    Unit unit = (Unit)data;
                    
                    GameObject goblin = Instantiate(unitPrefs[0], ((GameObject)tmp[0]).transform);
                    goblin.transform.position = hit.transform.position;

                    ingameDeckShuffler.UseCard(selectedObject.GetComponent<Index>().Id);
                }
                else if(data.GetType() == typeof(Skill)) {
                    Skill skill = (Skill)data;

                }
            }
        }
    }
}
