using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using System.Linq;
using System;
using Spine.Unity;
using UnityEngine.UI;

public class UpgradableBuildingGetter : MonoBehaviour {
    PlayerController playerController;
    IngameCityManager icm;
    IngameSceneEventHandler eventHandler;

    [Header("Upgrade Modal Components")]
    [SerializeField] GameObject upgradeModal;
    [SerializeField] Transform upgradeModal_content;
    [SerializeField] GameObject upgradeModal_content_item_pref;
    List<GameObject> buildingInfos;

    void Awake() {
        eventHandler = IngameSceneEventHandler.Instance;
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.RESOURCE_CHANGE, OnResourceChange);
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, OnMyBuildingsAdded);
    }

    // Use this for initialization
    void Start() {
        playerController = GetComponent<PlayerController>();
        if (playerController == null) Debug.LogError("PlayerController를 찾을 수 없습니다.");
        icm = playerController.IngameCityManager;
    }

    bool canInvoke = true;
    float coolTime = 1.0f;
    float time = 0;

    void Update() {
        if (!canInvoke) {
            time += Time.deltaTime;
            if (time > coolTime) {
                canInvoke = true;
                time = 0;
            }
        }
    }

    void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.RESOURCE_CHANGE, OnResourceChange);
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, OnMyBuildingsAdded);
    }

    private void OnMyBuildingsAdded(Enum Event_Type, Component Sender, object Param) {
        buildingInfos = (List<GameObject>)Param;
    }

    private void OnResourceChange(Enum Event_Type, Component Sender, object Param) {
        if (!playerController.isUpgradeModalActivate()) return;
        if (!canInvoke) return;

        var playerResource = (PlayerController.PlayerResource)Param;

        //Debug.Log("자원 변동 이벤트 발생");
        canInvoke = false;

        foreach(GameObject building in buildingInfos) {
            BuildingObject buildingObject = building.GetComponent<BuildingObject>();
            Card card = buildingObject.data.card;

            GameObject item = Instantiate(upgradeModal_content_item_pref, upgradeModal_content);
            item.transform.localScale = new Vector3(1, 1, 1);

            Transform newDataArea = item.transform.Find("Data/NewData").transform;
            Text Name = newDataArea.Find("Name/Val").GetComponent<Text>();
            Text Hp = newDataArea.Find("Specs/Hp/Val").GetComponent<Text>();
            Text Food = newDataArea.Find("Specs/Food/Val").GetComponent<Text>();
            Text Gold = newDataArea.Find("Specs/Gold/Val").GetComponent<Text>();
            Text Eco = newDataArea.Find("Specs/Eco/Val").GetComponent<Text>();

            Transform costArea = item.transform.Find("Data/Cost").transform;
            Text CostFood = costArea.Find("Data/Food/Val").GetComponent<Text>();
            Text CostGold = costArea.Find("Data/Gold/Val").GetComponent<Text>();
            Text CostEco = costArea.Find("Data/Eco/Val").GetComponent<Text>();

            Resource costs = CalcCost(card.lv, card.rarity);
            CostFood.text = costs.food.ToString();
            CostGold.text = costs.gold.ToString();
            CostEco.text = costs.environment.ToString();

            Name.text = buildingObject.data.card.name;

            if (!CanUpgrade(buildingObject, costs)) {
                item.transform.Find("Deactive").gameObject.SetActive(true);
            }
        }
    }

    //public List<GameObject> GetUpgradableBuildingList() {
    //    //HQ의 LV 이하까지 업그레이드 가능
    //    List<GameObject> list = new List<GameObject>();
    //    foreach (IngameCityManager.BuildingInfo info in icm.myBuildingsInfo) {
    //        list.Add(info.gameObject);
    //    }
    //    //List<GameObject> exceptList = list.FindAll(x => x.GetComponent<BuildingObject>().data.card.rareity > playerController.hqLevel);
    //    //list.Except(exceptList, new MyEqualityComparere());
    //    list.RemoveAll(x => x.GetComponent<BuildingObject>().data.card.rareity > playerController.hqLevel);

    //    foreach (GameObject building in list.ToList()) {
    //        BuildingObject buildingObject = building.GetComponent<BuildingObject>();

    //        int lv = buildingObject.data.card.lv;
    //        int rarerity = buildingObject.data.card.rareity;

    //        Resource resource = CalcCost(lv, rarerity);
    //        if (!isEnoughResource(resource)) {
    //            list.Remove(building);
    //        }
    //    }
    //    return list;
    //}

    public Resource CalcCost(int lv, int rarerity) {
        Resource resources = new Resource();

        int foodCost = Convert.ToInt32(300.0f * (rarerity + lv) / 2.0f);
        int goldCost = Convert.ToInt32(300.0f * (rarerity + lv) / 2.0f);
        int envCost = Convert.ToInt32(30.0f * (rarerity + lv) / 2.0f);

        resources.food = foodCost;
        resources.gold = goldCost;
        resources.environment = envCost;

        return resources;
    }

    public bool isEnoughResource(Resource resource) {
        if (playerController.resourceClass.food < resource.food) return false;
        if (playerController.resourceClass.gold < resource.gold) return false;
        if (playerController.resourceClass.environment < resource.environment) return false;
        return true;
    }

    public bool CanUpgrade(BuildingObject building, Resource resource) {
        if (building.data.card.rarity > playerController.hqLevel) return false;
        return isEnoughResource(resource);
    }
}