using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using System;
using UnityEngine.UI;

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

    public void InitUnitCard() {
        foreach (Unit unit in tileGroup.units) {
            GameObject card = Instantiate(cardPref, cardParent);
            card.transform.Find("Name").GetComponent<Text>().text = unit.name;
            card.GetComponent<IngameCard>().data = unit;
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
