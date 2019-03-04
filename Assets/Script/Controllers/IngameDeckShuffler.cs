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

    [SerializeField] GameObject 
        unitCardPref,
        spellCardPref;
    [SerializeField] Transform cardParent;
    [SerializeField] GameObject refreshCardBtn;

    private static int HAND_MAX_COUNT = 5;
    private readonly System.Random rand = new System.Random((int)DateTime.Now.Ticks);

    [SerializeField] List<GameObject> origin = new List<GameObject>();    //원본 액티브 카드 리스트
    [SerializeField] List<int> Deck = new List<int>();  //덱 인덱스 리스트
    [SerializeField] List<int> Hand = new List<int>();  //핸드 인덱스 리스트

    TileGroup tileGroup;
    void Awake() {
        ingameCityManager = GetComponent<IngameCityManager>();
        eventHandler = IngameSceneEventHandler.Instance;
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.UNIT_UPGRADED, OnUnitUpgraded);
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, OnHqUpgraded);
    }

    void Start() {
        tileGroup = transform.GetChild(1).GetComponent<TileGroup>();
        InitCard();
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
        InitCard();
    }

    public void DeactiveCard(string id, GameObject parentBuilding) {
        GameObject card = origin.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == parentBuilding);
        if (card == null) return;
        int index = card.GetComponent<Index>().Id;
        Deck.Remove(index);
        Hand.Remove(index);
        card.SetActive(false);
    }

    public void ActivateCard(string id, GameObject parentBuilding) {
        GameObject card = origin.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == parentBuilding);
        if (card == null) return;
        int index = card.GetComponent<Index>().Id;
        Deck.Add(index);

        RefillCard(index);
    }

    public void InitCard() {
        int index = 0;
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

            card.AddComponent<Index>().Id = index;
            card.SetActive(false);
            origin.Add(card);
            index++;
        }
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

            card.AddComponent<Index>().Id = index;
            origin.Add(card);
            card.SetActive(false);
            index++;
        }

        HandDeck();
    }

    //initialize
    public void HandDeck() {
        List<int> pool = new List<int>();
        foreach(GameObject item in origin) {
            ActiveCardInfo info = item.GetComponent<ActiveCardInfo>();
            if(canUseCard(info)) pool.Add(item.GetComponent<Index>().Id);
        }
        //IEnumerable<int> query =
        //    from x in origin
        //    select x.GetComponent<Index>().Id;
        //var pool = query.ToList();
        //foreach(int asdsa in query) {
        //    Debug.Log(asdsa);
        //}
        int[] rndNums = null;
        if (pool.Count > HAND_MAX_COUNT) {
            rndNums = RndNumGenerator.getRandomInt(HAND_MAX_COUNT, pool.ToArray());
        }
        else rndNums = pool.ToArray();

        Deck.Clear();
        Hand.Clear();

        for (int i = 0; i < rndNums.Length; i++) {
            Hand.Add(rndNums[i]);
            origin[rndNums[i]].SetActive(true);
        }
    }

    public void RefillCard(int id) {
        if (Hand.Count == HAND_MAX_COUNT) return;
        if (Deck.Count == 0) Deck.Add(id);
        var choiceIndex = rand.Next(Deck.Count);
        var choice = Deck[choiceIndex];

        //Debug.Log("Choice : " + choice);
        Deck.RemoveAt(choiceIndex);
        Hand.Add(choice);
        origin[choice].SetActive(true);
        origin[choice].transform.SetAsLastSibling();
    }

    //card use
    public void UseCard(GameObject selectedObject) {
        int id = selectedObject.GetComponent<Index>().Id;
        //Debug.Log("ID : " + id);

        var match = origin.Find(x => id == x.GetComponent<Index>().Id);
        if (match == null) return;

        match.SetActive(false);
        Hand.Remove(id);

        RefillCard(id);

        ActiveCard activeCard = selectedObject.GetComponent<ActiveCardInfo>().data;
        ActiveCardCoolTime cooltimeComp = activeCard.parentBuilding.AddComponent<ActiveCardCoolTime>();
        cooltimeComp.cards = origin;

        if (!string.IsNullOrEmpty(activeCard.unit.name)) {
            cooltimeComp.coolTime = activeCard.unit.coolTime;
        }
        else if (!string.IsNullOrEmpty(activeCard.skill.name)) {
            cooltimeComp.coolTime = activeCard.skill.coolTime;
        }

        cooltimeComp.StartCool();
    }

    private bool canUseCard(ActiveCardInfo data) {
        Cost cost = data.data.unit.cost;
        Unit unit = data.data.unit;
        Skill skill = data.data.skill;

        if (!string.IsNullOrEmpty(unit.name)) {
            if (playerController.hqLevel >= unit.tierNeed) return true;
        }

        else if (!string.IsNullOrEmpty(skill.name)) {
            if (playerController.hqLevel >= skill.tierNeed) return true;
        }
        return false;
    }

    //(핸드)카드 교체 기능
    //쿨타임 30초
    public void HandReset() {
        //쿨타임이면 return 
        //IngameHandChangeCoolTime prevComp = playerController.gameObject.GetComponent<IngameHandChangeCoolTime>();
        //if (prevComp != null) return;

        //List<GameObject> tmp = new List<GameObject>();
        //foreach(GameObject item in Hand) {
        //    tmp.Add(item);
        //}
        //Hand.Clear();
        //for(int i = 0; i < 5; i++) {
        //    RefillCard();
        //}

        //int count = Hand.Count;
        //if(count < 5) {
        //    Deck.AddRange(tmp);
        //    for(int i = 0; i < 5 - count; i++) {
        //        RefillCard();
        //    }
        //}

        //playerController.resourceClass.turn--;
        //playerController.PrintResource();

        //IngameHandChangeCoolTime coolComp = playerController.gameObject.AddComponent<IngameHandChangeCoolTime>();
        //coolComp.coolTime = 30;
        //coolComp.Btn = refreshCardBtn;
        //coolComp.StartCool();

        //MakeCardPrefab();
    }
}
