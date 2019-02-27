using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using System;
using UnityEngine.UI;
using System.Text;
using System.Linq;

public class IngameDeckShuffler : MonoBehaviour {
    IngameCityManager ingameCityManager;
    [SerializeField] PlayerController playerController;
    IngameSceneEventHandler eventHandler;

    List<GameObject> cards;
    [SerializeField] GameObject 
        unitCardPref,
        spellCardPref;
    [SerializeField] Transform cardParent;

    private int handCount;
    private readonly System.Random rand = new System.Random((int)DateTime.Now.Ticks);
    public List<GameObject> Deck = new List<GameObject>();  //덱 뭉치
    public List<GameObject> Hand = new List<GameObject>();  //핸드

    TileGroup tileGroup;
    void Awake() {
        ingameCityManager = GetComponent<IngameCityManager>();
        eventHandler = IngameSceneEventHandler.Instance;
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.UNIT_UPGRADED, OnUnitUpgraded);
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, OnHqUpgraded);
    }

    void Start() {
        tileGroup = transform.GetChild(1).GetComponent<TileGroup>();
        cards = new List<GameObject>();
        InitUnitCard();
        InitSkillCard();

        HandDeck(cards);
    }

    void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.UNIT_UPGRADED, OnHqUpgraded);
    }

    private void OnUnitUpgraded(Enum Event_Type, Component Sender, object Param) {
        //Unit targetUnit = (Unit)Param;
        //foreach(GameObject card in cards) {
        //    object data = card.GetComponent<IngameCard>().data;
        //    if(data.GetType() == typeof(Unit)) {
        //        Unit unit = (Unit)data;
        //        if(unit.id == targetUnit.id) {
        //            card.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = targetUnit.lv.ToString();
        //        }
        //    }
        //}
    }

    private void OnHqUpgraded(Enum Event_Type, Component Sender, object Param) {
        Hand.Clear();
        Deck.Clear();

        InitUnitCard();
        InitSkillCard();
        MakeCardPrefab();
    }

    public void DeactiveCard(string id, GameObject parentBuilding) {
        GameObject card = Hand.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == parentBuilding);
        if (card == null) return;

        card.SetActive(false);
    }

    public void ActivateCard(string id, GameObject parentBuilding) {
        GameObject card = Hand.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == parentBuilding);
        if (card == null) return;

        card.SetActive(true);
    }

    public void InitUnitCard() {
        foreach (ActiveCard unitCard in tileGroup.units) {
            Unit unit = unitCard.unit;
            if (unit.tierNeed <= playerController.hqLevel) {
                GameObject card = Instantiate(unitCardPref, cardParent);
                card.transform.Find("Name/Value").GetComponent<Text>().text = unit.name;
                ActiveCardInfo activeCardInfo = card.AddComponent<ActiveCardInfo>();
                activeCardInfo.data = unitCard;
                card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "unit", unit.name);

                if (unit.cost.food > 0) card.transform.Find("Cost/FoodIcon/Value").GetComponent<Text>().text = unit.cost.food.ToString();
                if (unit.cost.gold > 0) card.transform.Find("Cost/GoldIcon/Value").GetComponent<Text>().text = unit.cost.gold.ToString();
                card.transform.Find("Tier/Value").GetComponent<Text>().text = unit.tierNeed.ToString();

                if (Hand.Count < 5) Hand.Add(card);
                else Deck.Add(card);
            }
        }
    }

    public void InitSkillCard() {
        foreach (ActiveCard spellCard in tileGroup.spells) {
            Skill skill = spellCard.skill;
            if (skill.tierNeed <= playerController.hqLevel) {
                GameObject card = Instantiate(spellCardPref, cardParent);
                card.transform.Find("Name/Value").GetComponent<Text>().text = skill.name;
                ActiveCardInfo activeCardInfo = card.AddComponent<ActiveCardInfo>();
                activeCardInfo.data = spellCard;

                card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "spell", skill.name);

                if (skill.cost.food > 0) card.transform.Find("Cost/FoodIcon/Value").GetComponent<Text>().text = skill.cost.food.ToString();
                if (skill.cost.gold > 0) card.transform.Find("Cost/GoldIcon/Value").GetComponent<Text>().text = skill.cost.gold.ToString();
                card.transform.Find("Tier/Value").GetComponent<Text>().text = skill.tierNeed.ToString();

                if (Hand.Count < 5) Hand.Add(card);
                else Deck.Add(card);
            }
        }
    }

    public void MakeCardPrefab() {
        foreach(Transform item in cardParent) {
            Destroy(item.gameObject);
        }
        int index = 0;
        foreach(GameObject gameObject in Hand) {
            ActiveCard activeCard = gameObject.GetComponent<ActiveCardInfo>().data;
            if (!string.IsNullOrEmpty(activeCard.unit.name)) {
                GameObject card = Instantiate(unitCardPref, cardParent);
                Hand[index] = card;
                Unit unit = activeCard.unit;
                card.transform.Find("Name/Value").GetComponent<Text>().text = unit.name;
                ActiveCardInfo activeCardInfo = card.AddComponent<ActiveCardInfo>();
                activeCardInfo.data = activeCard;
                card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "unit", unit.name);

                if (unit.cost.food > 0) card.transform.Find("Cost/FoodIcon/Value").GetComponent<Text>().text = unit.cost.food.ToString();
                if (unit.cost.gold > 0) card.transform.Find("Cost/GoldIcon/Value").GetComponent<Text>().text = unit.cost.gold.ToString();
                card.transform.Find("Tier/Value").GetComponent<Text>().text = unit.tierNeed.ToString();

                IngameCityManager.BuildingInfo building = ingameCityManager.myBuildingsInfo.Find(x => x.gameObject == gameObject.GetComponent<ActiveCardInfo>().data.parentBuilding);
                if (!building.activate) card.SetActive(false);
            }
            else {
                GameObject card = Instantiate(spellCardPref, cardParent);
                Hand[index] = card;
                Skill skill = activeCard.skill;
                card.transform.Find("Name/Value").GetComponent<Text>().text = skill.name;
                ActiveCardInfo activeCardInfo = card.AddComponent<ActiveCardInfo>();
                activeCardInfo.data = activeCard;

                card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "spell", skill.name);

                if (skill.cost.food > 0) card.transform.Find("Cost/FoodIcon/Value").GetComponent<Text>().text = skill.cost.food.ToString();
                if (skill.cost.gold > 0) card.transform.Find("Cost/GoldIcon/Value").GetComponent<Text>().text = skill.cost.gold.ToString();
                card.transform.Find("Tier/Value").GetComponent<Text>().text = skill.tierNeed.ToString();

                IngameCityManager.BuildingInfo building = ingameCityManager.myBuildingsInfo.Find(x => x.gameObject == gameObject.GetComponent<ActiveCardInfo>().data.parentBuilding);
                if (!building.activate) card.SetActive(false);
            }
            index++;
        }
    }

    //initialize
    public void HandDeck(List<GameObject> cards, int handCount = 5) {
        this.handCount = handCount;
        for (int i = 0; Deck.Count > 0 && i < handCount; i++) {
            RefillCard();
        }
    }

    public void RefillCard() {
        var choiceIndex = rand.Next(Deck.Count);
        var choice = Deck[choiceIndex];
        Deck.RemoveAt(choiceIndex);
        Hand.Add(choice);
    }

    //card use
    public void UseCard(GameObject selectedObject) {
        int handIndex = selectedObject.transform.GetSiblingIndex();
        Deck.Add(Hand[handIndex]);
        Hand.RemoveAt(handIndex);

        RefillCard();
        MakeCardPrefab();

        ActiveCard activeCard = selectedObject.GetComponent<ActiveCardInfo>().data;
        ActiveCardCoolTime cooltimeComp = activeCard.parentBuilding.AddComponent<ActiveCardCoolTime>();

        if (!string.IsNullOrEmpty(activeCard.unit.name)) {
            cooltimeComp.coolTime = activeCard.unit.coolTime;
        }
        else if (!string.IsNullOrEmpty(activeCard.skill.name)) {
            cooltimeComp.coolTime = activeCard.skill.coolTime;
        }
        cooltimeComp.Hand = Hand;
        cooltimeComp.Deck = Deck;
        cooltimeComp.StartCool();
    }

    //(핸드)카드 교체 기능
    public void HandReset() {
        List<GameObject> tmp = new List<GameObject>();
        tmp = Hand;
        Hand = Deck;
        Deck = tmp;

        MakeCardPrefab();
    }
}
