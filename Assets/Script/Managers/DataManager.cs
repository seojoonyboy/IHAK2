using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using UnityEngine.UI;

public class DataManager : Singleton<DataManager> {
    protected DataManager() { }

    Dictionary<Building.Category, List<GameObject>> buildings;
    Dictionary<int, Image> buildingImages;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        MakeBuildingObjects();
        GetComponent<BuildingImages>().SetImages();
    }

    private void MakeBuildingObjects() {
        List<Building> result = JsonReader.Read("Buildings", new Building());

        buildings = new Dictionary<Building.Category, List<GameObject>>();
        List<GameObject> products = new List<GameObject>();
        List<GameObject> militaries = new List<GameObject>();
        List<GameObject> specials = new List<GameObject>();

        foreach (Building item in result) {
            GameObject obj = new GameObject();
            obj.name = item.Name;
            obj.transform.SetParent(transform.Find("BuildingObjects").transform);
            BuildingObject buildingObject = obj.AddComponent<BuildingObject>();
            obj.AddComponent<SpriteRenderer>();
            buildingObject.data = item;
            
            if(item.Type == "Product") products.Add(obj);
            else if(item.Type == "Military") militaries.Add(obj);
            else if(item.Type == "Special") specials.Add(obj);
        }

        buildings[Building.Category.PRODUCT] = products;
        buildings[Building.Category.MILITARY] = militaries;
        buildings[Building.Category.SPECIAL] = specials;
    }

    public GameObject GetBuildingObject(Building.Category type, int id) {
        List<GameObject> sectedCategoryBuildings = buildings[type];
        if (sectedCategoryBuildings == null) return null;
        foreach(GameObject obj in sectedCategoryBuildings) {
            Card card = obj.GetComponent<BuildingObject>().data;
            if(card.Id == id) {
                return obj;
            }
        }
        return null;
    }

    public List<GameObject> GetBuildingObjects(Building.Category type) {
        return buildings[type];
    }
}
