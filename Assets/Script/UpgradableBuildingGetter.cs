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

    private const int MAX_LV = 4;

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
        //if (!canInvoke) {
        //    time += Time.deltaTime;
        //    if (time > coolTime) {
        //        canInvoke = true;
        //        time = 0;
        //    }
        //}
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

    }

    private List<GameObject> Filter(List<GameObject> data) {
        var groupByGold = data
            .OrderBy(x => x.GetComponent<IngameUpgradeCard>().cost.gold)
            .GroupBy(group => group.GetComponent<IngameUpgradeCard>().cost.gold)
            .ToList();
        data.Clear();

        foreach (var result in groupByGold) {
            List<GameObject> list = result.ToList();
            list.Sort((x, y) => x.GetComponent<IngameUpgradeCard>().targetBuilding.GetComponent<BuildingObject>().data.card.name.CompareTo(
                y.GetComponent<IngameUpgradeCard>().targetBuilding.GetComponent<BuildingObject>().data.card.name)
            );
            data.AddRange(list);
        }

        return data;
    }

    private void ClearList() {
        foreach(Transform tf in upgradeModal_content) {
            Destroy(tf.gameObject);
        }
    }

    public void CloseModal() {
        upgradeModal.SetActive(false);
    }

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
        //if (playerController.resourceClass.environment < resource.environment) return false;
        return true;
    }

    public bool CanUpgrade(BuildingObject building, Resource resource) {
        if (building.data.card.lv >= playerController.hqLevel) return false;
        if (building.data.card.lv >= MAX_LV) return false;
        if (building.data.card.id == "primal_town_center" && playerController.hqLevel == MAX_LV) return false;
        return isEnoughResource(resource);
    }
    /*
    public void SameSorting(List<GameObject> upgradeList) {
        int maxCount = upgradeList.Capacity;
        int pivot = 0;
        int targetLocation = 0;
        GameObject tempLocation;

        for (int i = 0; i<maxCount; i++) {
            pivot = upgradeList.BinarySearch(upgradeList[i]);

            for(int j = pivot + 1; j < maxCount; j++) {
                if(upgradeList[i].GetComponent<IngameUpgradeCard>().name == upgradeList[j].GetComponent<IngameUpgradeCard>().name) {
                    targetLocation = upgradeList.BinarySearch(upgradeList[j]);

                }
            }
        }
    }
    */

    public class MyComparer : IComparer<int> {
        public int Compare(int x, int y) {
            if (x > y)
                return 1;
            if (x < y)
                return -1;
            else
                return 0;
        }
    }
}