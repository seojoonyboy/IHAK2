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

    private const int MAX_LV = 3;

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
        //if (!canInvoke) return;

        ClearList();
        var playerResource = (PlayerController.PlayerResource)Param;

        //Debug.Log("자원 변동 이벤트 발생");
        canInvoke = false;

        List<GameObject> availableItems = new List<GameObject>();
        List<GameObject> unavailableItems = new List<GameObject>();
        GameObject hqItem = null;
        foreach(GameObject building in buildingInfos) {
            BuildingObject buildingObject = building.GetComponent<BuildingObject>();
            Card card = buildingObject.data.card;

            GameObject item = Instantiate(upgradeModal_content_item_pref, upgradeModal_content);
            item.transform.localScale = new Vector3(1, 1, 1);
            IngameUpgradeCard ingameUpgradeCard = item.AddComponent<IngameUpgradeCard>();
            ingameUpgradeCard.targetBuilding = building;

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
            //GameObject.Find("ConstructManager").GetComponent<ConstructManager>().GetComponent<BuildingImages>().GetImage("primal", "upgrade", buildingObject.data)
            Image UpgradeImg = item.transform.Find("Image").GetComponent<Image>();
            //Text Lv = item.transform.Find("Image/Lv").GetComponent<Text>();
            int lv = 0;

            if(card.id == "primal_town_center") {
                if (card.lv == MAX_LV) lv = playerController.hqLevel;
                else lv = playerController.hqLevel;
                hqItem = item;
            }
            else lv = card.lv;

            //Lv.text = "Lv " + lv;
            Resource costs = new Resource();
            if (card.id == "primal_town_center") {
                if(playerController.hqLevel == 1) {
                    costs.food = icm.hq_tier_2.upgradeCost.food;
                    costs.gold = icm.hq_tier_2.upgradeCost.gold;
                    costs.environment = icm.hq_tier_2.upgradeCost.env;
                }
                else if(playerController.hqLevel == 2) {
                    costs.food = icm.hq_tier_3.upgradeCost.food;
                    costs.gold = icm.hq_tier_3.upgradeCost.gold;
                    costs.environment = icm.hq_tier_3.upgradeCost.env;
                }
            }
            else {
                costs = CalcCost(lv, card.rarity);
            }
            ingameUpgradeCard.cost = costs;

            CostFood.text = costs.food.ToString();
            CostGold.text = costs.gold.ToString();
            CostEco.text = costs.environment.ToString();
            UpgradeImg.sprite = buildingObject.upgradeIcon;

            Name.text = buildingObject.data.card.name + " Lv." + lv;
            if (!CanUpgrade(buildingObject, costs)) {
                item.transform.Find("Deactive").gameObject.SetActive(true);
                unavailableItems.Add(item);
            }
            else {
                availableItems.Add(item);
            }

            Button upgradeBtn = costArea.Find("Button").GetComponent<Button>();
            upgradeBtn.onClick.AddListener(() => Modal.instantiate(
                Name.text + "를 업그레이드 하시겠습니까?", 
                Modal.Type.YESNO, 
                () => {
                    if(card.id == "primal_town_center") {
                        GetComponent<PlayerController>().HqUpgrade();
                        card.lv += 1;
                        if (buildingObject.spine != null) buildingObject.GetComponent<TileSpineAnimation>().Upgrade();
                    }
                    else playerController.Upgrade(item, costs);
                }
            ));

            ingameUpgradeCard.lv = card.lv;
            ingameUpgradeCard.newIncreasePower = new Resource();

            int foodIncreaseAmount = 0;
            int goldIncreaseAmount = 0;
            int envIncreaseAmount = 0;
            if (card.id == "primal_town_center") {
                if(playerController.hqLevel == 1) {
                    foodIncreaseAmount = icm.hq_tier_2.product.food - icm.hq_tier_1.product.food;
                    goldIncreaseAmount = icm.hq_tier_2.product.gold - icm.hq_tier_1.product.gold;
                    envIncreaseAmount = icm.hq_tier_2.product.env - icm.hq_tier_1.product.env;
                }
                else if(playerController.hqLevel == 2) {
                    foodIncreaseAmount = icm.hq_tier_3.product.food - icm.hq_tier_2.product.food;
                    goldIncreaseAmount = icm.hq_tier_3.product.gold - icm.hq_tier_2.product.gold;
                    envIncreaseAmount = icm.hq_tier_3.product.env - icm.hq_tier_2.product.env;
                }
            }
            else {
                foodIncreaseAmount = Convert.ToInt32(card.product.food * (lv / 13.0f + card.rarity / 13.0f));
                goldIncreaseAmount = Convert.ToInt32(card.product.gold * (lv / 13.0f + card.rarity / 13.0f));
                envIncreaseAmount = Convert.ToInt32(card.product.environment * (lv / 13.0f + card.rarity / 13.0f));
            }
            

            ingameUpgradeCard.newIncreasePower.food = foodIncreaseAmount;
            ingameUpgradeCard.newIncreasePower.gold = goldIncreaseAmount;
            ingameUpgradeCard.newIncreasePower.environment = envIncreaseAmount;

            //Todo : 생산력 표기
            if (foodIncreaseAmount >= 0) Food.text = "+" + foodIncreaseAmount;
            else Food.text = foodIncreaseAmount.ToString();
            if (goldIncreaseAmount >= 0) Gold.text = "+" + goldIncreaseAmount;
            else Gold.text = goldIncreaseAmount.ToString();
            if (envIncreaseAmount >= 0) Eco.text = "+" + envIncreaseAmount;
            else Eco.text = envIncreaseAmount.ToString();

            int newHp = Convert.ToInt32(card.hitPoint * (1 + (lv / 10.0f) + (card.rarity / 10.0f)));
            int HpIncreaseAmount = newHp - card.hitPoint;
            if (HpIncreaseAmount > 0) Hp.text = "+" + HpIncreaseAmount;
            else Hp.text = HpIncreaseAmount.ToString();
            
            ingameUpgradeCard.newHp = newHp;
        }

        unavailableItems = unavailableItems.OrderBy(x => x.GetComponent<IngameUpgradeCard>().cost.gold).ToList();
        foreach (GameObject building in unavailableItems) {
            building.transform.SetAsFirstSibling();
        }

        availableItems = availableItems.OrderBy(x => x.GetComponent<IngameUpgradeCard>().cost.gold).ToList();
        foreach(GameObject building in availableItems) {
            building.transform.SetAsFirstSibling();
        }

        if(hqItem != null) hqItem.transform.SetAsFirstSibling();
    }

    private void ClearList() {
        foreach(Transform tf in upgradeModal_content) {
            Destroy(tf.gameObject);
        }
    }

    public void CloseModal() {
        upgradeModal.SetActive(false);
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
        Debug.Log(playerController.hqLevel);
        if (building.data.card.lv >= playerController.hqLevel) return false;
        if (building.data.card.lv >= 3) return false;
        if (building.data.card.id == "primal_town_center" && playerController.hqLevel == MAX_LV) return false;
        return isEnoughResource(resource);
    }

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