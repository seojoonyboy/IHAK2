using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using DataModules;
using UnityEngine.UI;
using System.Text;
using TMPro;
using System;
using Spine;
using Spine.Unity;

public class ConstructManager : Singleton<ConstructManager> {
    protected ConstructManager() { }

    Dictionary<string, List<GameObject>> buildings;
    Dictionary<int, Image> buildingImages;
    NetworkManager _networkManager;
    public GameObject townCenter;
    public GameObject hpGauge;
    public GameObject nameMesh;
    public GameObject unfog;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        _networkManager = NetworkManager.Instance;
    }

    public List<GameObject> GetBuildingObjects(string type) {
        return buildings[type];
    }

    public List<GameObject> GetBuildingObjects() {
        List<GameObject> result = new List<GameObject>();
        result.AddRange(buildings["hero"]);
        result.AddRange(buildings["active"]);
        result.AddRange(buildings["passive"]);

        return result;
    }

    /// <summary>
    /// 내 인벤토리의 카드를 찾는 용도
    /// </summary>
    /// <param name="id">내 인벤토리의 카드 번호</param>
    /// <returns></returns>
    public GameObject GetBuildingObjectById(int id) {
        var buildings = GetBuildingObjects();
        var result = buildings.Find(x => x.GetComponent<BuildingObject>().card.id == id);
        return result;
    }

    /// <summary>
    /// ConstructManager의 특정 BuildingObject를 찾는 용도
    /// </summary>
    /// <param name="id">건물의 고유 식별번호</param>
    /// <returns></returns>
    public GameObject GetBuildingObjectById(string id) {
        var buildings = GetBuildingObjects();
        var result = buildings.Find(x => x.GetComponent<BuildingObject>().card.data.id == id);
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
        if (response.responseCode == 200) {
            var result = Req_cardsInventoryRead.Read<List<Req_cardsInventoryRead.Card>>(response.data.ToString());

            buildings = new Dictionary<string, List<GameObject>>();
            List<GameObject> heroes = new List<GameObject>();
            List<GameObject> actives = new List<GameObject>();
            List<GameObject> passive = new List<GameObject>();

            foreach (Card card in result) {
                GameObject obj = new GameObject();
                obj.name = card.data.name;
                obj.transform.SetParent(transform.Find("BuildingObjects").transform);
                BuildingObject buildingObject = obj.AddComponent<BuildingObject>();
                obj.GetComponent<BuildingObject>().setTileLocation = -1;
                obj.AddComponent<PolygonCollider2D>().points = new Vector2[4] { new Vector2(0, 10), new Vector2(-10, 0), new Vector2(0, -10), new Vector2(10, 0) };
                obj.tag = "Building";
                GameObject gauge = Instantiate(hpGauge, obj.transform);
                GameObject buildingname = Instantiate(nameMesh, obj.transform);
                buildingname.transform.SetAsLastSibling();
                buildingname.GetComponent<TextMeshPro>().text = obj.GetComponent<BuildingObject>().name;
                gauge.SetActive(false);
                buildingObject.card = card;

                if (card.data.type == "hero") heroes.Add(obj);
                else if (card.data.type == "active") actives.Add(obj);
                else if (card.data.type == "passive") passive.Add(obj);
            }

            buildings["hero"] = heroes;
            buildings["active"] = actives;
            buildings["passive"] = passive;

            GetComponent<BuildingImages>().SetImages();
        }
    }
}
