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
    public List<GameObject> Deck;                           //덱 뭉치
    public List<GameObject> Hand;                           //핸드
    public List<GameObject> Grave = new List<GameObject>(); //무덤

    TileGroup tileGroup;
    void Awake() {
        ingameCityManager = GetComponent<IngameCityManager>();
        eventHandler = IngameSceneEventHandler.Instance;
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.UNIT_UPGRADED, OnUnitUpgraded);
    }

    void Start() {
        tileGroup = transform.GetChild(1).GetComponent<TileGroup>();
        cards = new List<GameObject>();
        InitUnitCard();
        InitSkillCard();

        HandDeck(cards);

        //UseCard(0);
        //UseCard(1);
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

    public void Clear() {
        foreach (Transform card in cardParent.transform) {
            cards.Remove(card.gameObject);
            Destroy(card.gameObject);
        }
    }

    public void DeactiveCard(string id, GameObject parentBuilding) {
        GameObject card = cards.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == parentBuilding);
        if (card == null) return;
        card.SetActive(false);
    }

    public void ActivateCard(string id, GameObject parentBuilding) {
        GameObject card = cards.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == parentBuilding);
        if (card == null) return;
        card.SetActive(true);
    }

    public void InitUnitCard() {
        foreach (ActiveCard unitCard in tileGroup.units) {
            Unit unit = unitCard.unit;
            GameObject card = Instantiate(unitCardPref, cardParent);
            card.transform.Find("Name/Value").GetComponent<Text>().text = unit.name;
            ActiveCardInfo activeCardInfo = card.AddComponent<ActiveCardInfo>();
            activeCardInfo.data = unitCard;
            card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "unit", unit.name);

            if (unit.cost.food > 0) card.transform.Find("Cost/FoodIcon/Value").GetComponent<Text>().text = unit.cost.food.ToString();
            if (unit.cost.gold > 0) card.transform.Find("Cost/GoldIcon/Value").GetComponent<Text>().text = unit.cost.gold.ToString();
            card.transform.Find("Tier/Value").GetComponent<Text>().text = unit.tierNeed.ToString();
            Debug.Log(playerController.hqLevel);
            if (unit.tierNeed > playerController.hqLevel) {
                card.SetActive(false);
            }
            cards.Add(card);
        }
    }

    public void InitSkillCard() {
        foreach (ActiveCard spellCard in tileGroup.spells) {
            Skill skill = spellCard.skill;
            GameObject card = Instantiate(spellCardPref, cardParent);
            card.transform.Find("Name/Value").GetComponent<Text>().text = skill.name;
            ActiveCardInfo activeCardInfo = card.AddComponent<ActiveCardInfo>();
            activeCardInfo.data = spellCard;

            card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "spell", skill.name);

            if (skill.cost.food > 0) card.transform.Find("Cost/FoodIcon/Value").GetComponent<Text>().text = skill.cost.food.ToString();
            if (skill.cost.gold > 0) card.transform.Find("Cost/GoldIcon/Value").GetComponent<Text>().text = skill.cost.gold.ToString();
            card.transform.Find("Tier/Value").GetComponent<Text>().text = skill.tierNeed.ToString();
            Debug.Log(playerController.hqLevel);
            if (skill.tierNeed > playerController.hqLevel) {
                card.SetActive(false);
            }
            cards.Add(card);
        }
    }

    //initialize
    public void HandDeck(List<GameObject> cards, int handCount = 5) {
        this.handCount = handCount;
        Deck = new List<GameObject>(cards);
        Hand = new List<GameObject>(handCount);
        for (int i = 0; Deck.Count > 0 && i < handCount; i++) {
            RefillCard();
        }
    }

    public void RefillCard() {
        var @where = Deck.Count > 0 ? Deck : Grave;
        var choiceIndex = rand.Next(@where.Count);
        var choice = @where[choiceIndex];
        @where.RemoveAt(choiceIndex);
        Hand.Add(choice);
        if (Grave.Count <= 0) return;
        Deck.AddRange(Grave);
        Grave.Clear();
    }

    //card use
    public void UseCard(int handIndex) {
        //Debug.Log(Hand[handIndex].GetComponent<IngameCard>().data.GetType());
        Grave.Add(Hand[handIndex]);
        Hand.RemoveAt(handIndex);

        RefillCard();
    }
}
