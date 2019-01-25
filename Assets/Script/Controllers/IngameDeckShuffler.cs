using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using System;
using UnityEngine.UI;
using System.Text;

public class IngameDeckShuffler : MonoBehaviour {
    IngameCityManager ingameCityManager;
    [SerializeField] PlayerController playerController;
    List<GameObject> cards;
    [SerializeField] GameObject cardPref;
    [SerializeField] Transform cardParent;

    private int handCount;
    private readonly System.Random rand = new System.Random((int)DateTime.Now.Ticks);
    public List<GameObject> Deck;                           //덱 뭉치
    public List<GameObject> Hand;                           //핸드
    public List<GameObject> Grave = new List<GameObject>(); //무덤

    TileGroup tileGroup;
    void Awake() {
        ingameCityManager = GetComponent<IngameCityManager>();
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

    public void Clear() {
        foreach (Transform card in cardParent.transform) {
            Destroy(card.gameObject);
        }
    }

    public void DeactiveCard(string id) {
        if (id == "wolves_den") {
            foreach (GameObject card in cards) {
                object data = card.GetComponent<IngameCard>().data;
                if (data.GetType() == typeof(Unit)) {
                    Unit unit = (Unit)data;
                    if (unit.name == "늑대") {
                        card.SetActive(false);
                    }
                }
            }
        }

        else if (id == "magma_altar") {
            foreach (GameObject card in cards) {
                object data = card.GetComponent<IngameCard>().data;
                if (data.GetType() == typeof(Skill)) {
                    Skill skill = (Skill)data;
                    if (skill.name == "마그마") {
                        card.SetActive(false);
                    }
                }
            }
        }
    }

    public void ActivateCard(string id) {
        if (id == "wolves_den") {
            foreach (GameObject card in cards) {
                object data = card.GetComponent<IngameCard>().data;
                if (data.GetType() == typeof(Unit)) {
                    Unit unit = (Unit)data;
                    if (unit.name == "늑대") {
                        card.SetActive(true);
                    }
                }
            }
        }

        else if (id == "magma_altar") {
            foreach (GameObject card in cards) {
                object data = card.GetComponent<IngameCard>().data;
                if (data.GetType() == typeof(Skill)) {
                    Skill skill = (Skill)data;
                    if (skill.name == "마그마") {
                        card.SetActive(true);
                    }
                }
            }
        }
    }

    public void InitUnitCard() {
        foreach (Unit unit in tileGroup.units) {
            GameObject card = Instantiate(cardPref, cardParent);
            card.transform.Find("Name").GetComponent<Text>().text = unit.name;
            card.GetComponent<IngameCard>().data = unit;
            card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "unit", unit.name);

            StringBuilder sb = new StringBuilder();
            if (unit.cost.food > 0) sb.Append("식량 : " + unit.cost.food + "\n");
            if (unit.cost.gold > 0) sb.Append("금 : " + unit.cost.gold);
            card.transform.Find("Cost").GetComponent<Text>().text = sb.ToString();
            card.transform.Find("Tier").GetComponent<Text>().text = unit.tearNeed + " 등급";
            if (unit.tearNeed > playerController.hqLevel) {
                card.SetActive(false);
            }
            cards.Add(card);
        }
    }

    public void InitSkillCard() {
        foreach (Skill skill in tileGroup.activeSkills) {
            GameObject card = Instantiate(cardPref, cardParent);
            card.transform.Find("Name").GetComponent<Text>().text = skill.name;
            card.GetComponent<IngameCard>().data = skill;
            card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "spell", skill.name);
            StringBuilder sb = new StringBuilder();
            if (skill.cost.food > 0) sb.Append("식량 : " + skill.cost.food + "\n");
            if (skill.cost.gold > 0) sb.Append("금 : " + skill.cost.gold);
            card.transform.Find("Cost").GetComponent<Text>().text = sb.ToString();
            card.transform.Find("Tier").GetComponent<Text>().text = skill.tierNeed + " 등급";
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
