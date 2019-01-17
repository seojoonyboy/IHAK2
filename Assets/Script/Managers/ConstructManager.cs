using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
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

    public GameObject GetBuildingObjectById(int id) {
        var buildings = GetBuildingObjects();
        var result = buildings.Find(x => x.GetComponent<BuildingObject>().data.id == id);
        return result;
    }

    public void SetAllBuildings() {
        StringBuilder url = new StringBuilder();
        url.Append(_networkManager.baseUrl);
        url.Append("api/users/deviceid/")
            .Append(AccountManager.Instance.DEVICEID)
            .Append("/cardsinventory");
        Debug.Log(url.ToString());
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
                obj.AddComponent<PolygonCollider2D>().points = new Vector2[4] { new Vector2(0, 10), new Vector2(-10, 0), new Vector2(0, -10), new Vector2(10, 0) };
                obj.tag = "Building";
                buildingObject.data = item;

                if (item.card.type == "prod") products.Add(obj);
                else if (item.card.type == "military") militaries.Add(obj);
                else if (item.card.type == "special") specials.Add(obj);
            }

            buildings["prod"] = products;
            buildings["military"] = militaries;
            buildings["special"] = specials;

            GetComponent<BuildingImages>().SetImages();
        }
    }
}
