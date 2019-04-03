using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using System;
using UnityEngine.UI;
using System.Text;
using System.Linq;
using Sirenix.OdinInspector;

public partial class IngameDeckShuffler : SerializedMonoBehaviour {
    IngameCityManager ingameCityManager;
    [SerializeField] [ReadOnly] PlayerController playerController;
    IngameSceneEventHandler eventHandler;

    [SerializeField] GameObject 
        unitCardPref,
        spellCardPref;
    [SerializeField] public Transform heroCardParent;
    [SerializeField] public Transform spellCardParent;
    [SerializeField] public Transform itemCardParent;

    public List<GameObject> heroCards;
    public List<GameObject> spellCards;

    private static int HAND_MAX_COUNT = 5;
    private readonly System.Random rand = new System.Random((int)DateTime.Now.Ticks);

    void Awake() {
        ingameCityManager = GetComponent<IngameCityManager>();
        eventHandler = IngameSceneEventHandler.Instance;
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, OnHqUpgraded);
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.BUILDING_DESTROYED, OnBuildingDestroyed);

        playerController = PlayerController.Instance;
    }

    void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, OnHqUpgraded);
    }

    public void HeroReturn(GameObject parentBuilding, bool isDead) {
        GameObject card = heroCards.Find(
            x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == parentBuilding
        );

        if(card == null) {
            Debug.LogError("사망한 영웅 유닛의 카드를 찾지 못하였습니다!");
            return;
        }

        int index = card.GetComponent<Index>().Id;
        //buildingInfos.activate = true;
        //buildingInfos.gameObject.GetComponent<TileSpineAnimation>().SetUnit(true);
        if (isDead) {
            if(parentBuilding.GetComponent<ActiveCardCoolTime>() == null) {
                ActiveCardCoolTime comp = parentBuilding.AddComponent<ActiveCardCoolTime>();
                comp.targetCard = card;
                comp.coolTime = CalculateHeroCoolTime(card.GetComponent<ActiveCardInfo>());
                comp.StartCool();
            }
        }
    }

    private float CalculateHeroCoolTime(ActiveCardInfo card) {
        float baseCool = card.data.baseSpec.unit.coolTime;
        //float magLv = card.data.ev.lv;
        //return baseCool * ((100f + magLv * 8f) / 100f);
        return baseCool;
    }

    public void HeroReturnBtnClicked() {
        eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, this);
    }

    private void OnBuildingDestroyed(Enum Event_Type, Component Sender, object Param) { }

    private void OnHqUpgraded(Enum Event_Type, Component Sender, object Param) {
        InitCard();
    }

    public void InitCard() {
        heroCards = new List<GameObject>();
        spellCards = new List<GameObject>();

        int index = 0;
        foreach (ActiveCard unitCard in playerController.playerActiveCards().unitCards()) {
            Unit unit = unitCard.baseSpec.unit;
            GameObject card = Instantiate(unitCardPref, heroCardParent);
            card.transform.Find("Name/Value").GetComponent<Text>().text = unit.name;
            ActiveCardInfo activeCardInfo = card.AddComponent<ActiveCardInfo>();
            activeCardInfo.data = unitCard;
            card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "unit", unit.name);

            //if (unit.cost.food > 0) card.transform.Find("Cost/FoodIcon/Value").GetComponent<Text>().text = unit.cost.food.ToString();
            if (unit.cost.gold > 0) card.transform.Find("Cost/GoldIcon/Value").GetComponent<Text>().text = unit.cost.gold.ToString();
            //card.transform.Find("Tier/Value").GetComponent<Text>().text = unit.tierNeed.ToString();

            heroCards.Add(card);

            card.AddComponent<Index>().Id = index;
            index++;
        }

        index = 0;
        foreach (ActiveCard spellCard in playerController.playerActiveCards().spellCards()) {
            Skill skill = spellCard.baseSpec.skill;
            GameObject card = Instantiate(spellCardPref, spellCardParent);

            card.transform.Find("Name/Value").GetComponent<Text>().text = skill.name;
            ActiveCardInfo activeCardInfo = card.AddComponent<ActiveCardInfo>();
            activeCardInfo.data = spellCard;

            card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "spell", skill.name);

            //if (skill.cost.food > 0) card.transform.Find("Cost/FoodIcon/Value").GetComponent<Text>().text = skill.cost.food.ToString();
            if (skill.cost.gold > 0) card.transform.Find("Cost/GoldIcon/Value").GetComponent<Text>().text = skill.cost.gold.ToString();
            card.transform.Find("Tier/Value").GetComponent<Text>().text = skill.tierNeed.ToString();

            card.AddComponent<Index>().Id = index;

            spellCards.Add(card);

            AddSkill(skill.method.methodName, card, skill.method.args, skill.coolTime);
            index++;
        }
    }

    //card use
    public void UseCard(GameObject selectedObject) {
        int id = selectedObject.GetComponent<Index>().Id;
        ActiveCardInfo activeCard = selectedObject.GetComponent<ActiveCardInfo>();
        if (CanUseCard(activeCard)) {
            switch (activeCard.data.type) {
                //영웅 유닛 카드는 사용시 아예 핸드, 덱에서 제외
                case "unit":
                    playerController.playerResource().UseGold(activeCard.data.baseSpec.unit.cost.gold);
                    playerController.HeroSummon(activeCard.data);

                    selectedObject.GetComponent<IngameDragHandler>().enabled = false;
                    break;
                //마법 주문 카드는 사용시 다시 덱에 들어감.
                case "active":
                    playerController.playerResource().UseGold(activeCard.data.baseSpec.skill.cost.gold);
                    break;
            }
        }
        else {
            IngameAlarm.instance.SetAlarm("자원이 부족합니다!");
        }
    }

    private bool CanUseCard(ActiveCardInfo card) {
        string type = card.data.type;
        Cost cost = null;
        switch (type) {
            case "unit":
                cost = card.data.baseSpec.unit.cost;
                break;
            case "active":
                cost = card.data.baseSpec.skill.cost;
                break;
        }
        if (cost == null) return false;
        if (cost.gold > playerController.playerResource().Gold) return false;
        
        return true;
    }

    /// <summary>
    /// 쿨타임 제거 버튼
    /// </summary>
    public void CancelCoolTimeBtnClicked(GameObject card) {
        ActiveCardInfo cardInfo = card.GetComponent<ActiveCardInfo>();
        int lv = cardInfo.data.ev.lv;
        ActiveCardCoolTime coolTime = cardInfo.data.parentBuilding.GetComponent<ActiveCardCoolTime>();
        if (coolTime == null) return;
        uint cost = coolTime.cancelCooltimeCost;

        coolTime.OnTime();
    }
}


public partial class IngameDeckShuffler : SerializedMonoBehaviour {
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<Effects, GameObject> effectModules;
    public Camera camera;

    private void AddSkill(string methodName, GameObject card, string args, int coolTime) {
        switch (methodName) {
            case "magma":
                Destroy(card.GetComponent<IngameDragHandler>());
                MagmaDragHandler magmaDragHandler = card.AddComponent<MagmaDragHandler>();

                magmaDragHandler.Init(
                    camera,
                    effectModules[Effects.skill_magma],
                    PlayerController.Instance.maps[PlayerController.Player.PLAYER_1].transform.parent,
                    card.GetComponent<ActiveCardInfo>().data.parentBuilding,
                    this,
                    args,
                    coolTime
                );
                break;

            case "herb_distribution":
                Destroy(card.GetComponent<IngameDragHandler>());
                HerbDragHandler herbDragHandler = card.AddComponent<HerbDragHandler>();

                herbDragHandler.Init(
                    camera,
                    effectModules[Effects.skill_herb],
                    PlayerController.Instance.maps[PlayerController.Player.PLAYER_1].transform.parent,
                    card.GetComponent<ActiveCardInfo>().data.parentBuilding,
                    this,
                    args,
                    coolTime
                );
                break;
            //case ""
        }
    }

    public enum Effects {
        skill_magma,
        skill_herb,
        skill_scaryOracle
    }
}