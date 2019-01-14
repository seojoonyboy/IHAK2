using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using UnityEngine.UI;
using System.Text;
using System;

public class ConstructManager : Singleton<ConstructManager> {
    protected ConstructManager() { }

    Dictionary<string, List<GameObject>> buildings;
    Dictionary<int, Image> buildingImages;
    NetworkManager _networkManager;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        _networkManager = NetworkManager.Instance;
    }

    private void Start() {
        SetAllBuildings();
        AccountManager.Instance.GetMyDecks();
    }

    public GameObject GetBuildingObject(string type, int id) {
        List<GameObject> sectedCategoryBuildings = buildings[type];
        if (sectedCategoryBuildings == null) return null;
        foreach(GameObject obj in sectedCategoryBuildings) {
            Card card = obj.GetComponent<BuildingObject>().data.card;
            if(card.id == id) {
                return obj;
            }
        }
        return null;
    }

    public List<GameObject> GetBuildingObjects(string type) {
        return buildings[type];
    }

    public List<GameObject> GetBuildingObjects() {
        List<GameObject> result = new List<GameObject>();
        result.AddRange(buildings["prod"]);
        result.AddRange(buildings["military"]);
        result.AddRange(buildings["special"]);

        return result;
    }

    private void SetAllBuildings() {
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl);
        url.Append("api/users/deviceid/")
            .Append(AccountManager.Instance.DEVICEID)
            .Append("/cardsinventory");
        _networkManager.request("GET", url.ToString(), OnSetAllBuildingsCallback, false);
    }

    private void OnSetAllBuildingsCallback(HttpResponse response) {
        if(response.responseCode == 200) {
            List<Building> result = JsonReader.Read(response.data.ToString(), new Building());

            buildings = new Dictionary<string, List<GameObject>>();
            List<GameObject> products = new List<GameObject>();
            List<GameObject> militaries = new List<GameObject>();
            List<GameObject> specials = new List<GameObject>();

            foreach (Building item in result) {
                GameObject obj = new GameObject();
                obj.name = item.card.name;
                obj.transform.SetParent(transform.Find("BuildingObjects").transform);
                BuildingObject buildingObject = obj.AddComponent<BuildingObject>();
                obj.AddComponent<SpriteRenderer>();
                buildingObject.data = item;

                if (item.card.type == "prod") products.Add(obj);
                else if (item.card.type == "military") militaries.Add(obj);
                else if (item.card.type == "special") specials.Add(obj);
            }

            buildings["prod"] = products;
            buildings["military"] = militaries;
            buildings["special"] = specials;
            AccountManager.Instance.SetTileObjects();
            SetSprite();
        }
    }

    public void SetSprite() {
        for(int i = 0; i<transform.GetChild(0).childCount; i++) {
            transform.GetChild(0).GetChild(i).GetComponent<BuildingObject>().icon = GetComponent<BuildingImages>().total_icons[i];
            transform.GetChild(0).GetChild(i).GetComponent<BuildingObject>().mainSprite = GetComponent<BuildingImages>().total_images[i];
        }
    }
}
