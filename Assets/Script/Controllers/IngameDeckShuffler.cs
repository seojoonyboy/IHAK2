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
    [SerializeField] List<int> Grave = new List<int>();   //Draw발동시 사용된 카드가 임시로 머무는 장소

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
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, OnHqUpgraded);
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

    public void HeroReturn(GameObject parentBuilding, bool isDead) {
        GameObject card = origin.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == parentBuilding);
        IngameCityManager.BuildingInfo buildingInfos = ingameCityManager.myBuildingsInfo.Find(x => x.tileNum == parentBuilding.GetComponent<BuildingObject>().setTileLocation);
        

        int index = card.GetComponent<Index>().Id;
        buildingInfos.activate = true;
        if (isDead) {
            ActiveCardCoolTime comp = parentBuilding.AddComponent<ActiveCardCoolTime>();
            comp.coolTime = CalculateHeroCoolTime(card.GetComponent<ActiveCardInfo>());
            comp.cards = origin;            
            comp.StartCool();
        }

        if (Hand.Count == HAND_MAX_COUNT) {
            Deck.Add(index);
        }
        else {
            Hand.Add(index);
            origin[index].SetActive(true);
            origin[index].transform.SetAsLastSibling();
        }
    }

    private float CalculateHeroCoolTime(ActiveCardInfo card) {
        float baseCool = card.data.baseSpec.unit.coolTime;
        float magLv = card.data.ev.lv;
        return baseCool * ((100f + magLv * 8f) / 100f);
    }

    public void HeroReturnBtnClicked() {
        eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, this);
    }

    private void OnHqUpgraded(Enum Event_Type, Component Sender, object Param) {
        InitCard();
    }

    public void DeactiveCard(GameObject parentBuilding) {
        GameObject card = origin.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == parentBuilding);
        if (card == null) return;

        Skill skill = card.GetComponent<ActiveCardInfo>().data.baseSpec.skill;

        if(card.GetComponent<IngameDragHandler>() == null) {
            if (!string.IsNullOrEmpty(skill.name)) {
                if (skill.method.methodName == "skill_unit_heal") {
                    card.GetComponent<HealArea>().CancelDrag();
                    card.GetComponent<HealArea>().enabled = false;
                }
            }
        }
        else {
            card.GetComponent<IngameDragHandler>().CancelDrag();
            card.GetComponent<IngameDragHandler>().enabled = false;
            card.transform.Find("Deactive").gameObject.SetActive(true);
        }
        card.transform.Find("Deactive/Button").gameObject.SetActive(false);

        LayoutRebuilder.ForceRebuildLayoutImmediate(cardParent.GetComponent<RectTransform>());
        card.SetActive(false);
    }

    public void ActivateCard(GameObject parentBuilding) {
        GameObject card = origin.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == parentBuilding);
        if (card == null) return;

        card.SetActive(true);
        Skill skill = card.GetComponent<ActiveCardInfo>().data.baseSpec.skill;
        if (!string.IsNullOrEmpty(skill.name)) {
            if (skill.method.methodName != "skill_magma" && skill.method.methodName != "skill_unit_heal") {
                card.GetComponent<IngameDragHandler>().enabled = false;
                card.transform.Find("Deactive").gameObject.SetActive(true);
                card.transform.Find("Deactive/Button").gameObject.SetActive(false);
            }
            else if(skill.method.methodName == "skill_unit_heal") {
                card.GetComponent<HealArea>().enabled = true;
                card.transform.Find("Deactive").gameObject.SetActive(false);
                card.transform.Find("Deactive/Button").gameObject.SetActive(true);
            }
        }
        else {
            card.GetComponent<IngameDragHandler>().enabled = true;
            card.transform.Find("Deactive").gameObject.SetActive(false);
            card.transform.Find("Deactive/Button").gameObject.SetActive(true);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardParent.GetComponent<RectTransform>());
    }

    public void InitCard() {
        foreach(GameObject card in origin) {
            Destroy(card);
        }
        origin.Clear();
        Deck.Clear();
        Hand.Clear();

        int index = 0;
        foreach (ActiveCard unitCard in tileGroup.units) {
            Unit unit = unitCard.baseSpec.unit;
            GameObject card = Instantiate(unitCardPref, cardParent);
            card.transform.Find("Deactive/Button").GetComponent<Button>().onClick.AddListener(
                () => CancelCoolTimeBtnClicked(card));

            card.transform.Find("Name/Value").GetComponent<Text>().text = unit.name + unitCard.parentBuilding.transform.parent.name;
            ActiveCardInfo activeCardInfo = card.AddComponent<ActiveCardInfo>();
            activeCardInfo.data = unitCard;
            card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "unit", unit.name);

            if (unit.cost.food > 0) card.transform.Find("Cost/FoodIcon/Value").GetComponent<Text>().text = unit.cost.food.ToString();
            if (unit.cost.gold > 0) card.transform.Find("Cost/GoldIcon/Value").GetComponent<Text>().text = unit.cost.gold.ToString();
            card.transform.Find("Tier/Value").GetComponent<Text>().text = unit.tierNeed.ToString();

            card.AddComponent<Index>().Id = index;
            card.SetActive(false);
            origin.Add(card);
            Deck.Add(index);
            index++;
        }
        foreach (ActiveCard spellCard in tileGroup.spells) {
            Skill skill = spellCard.baseSpec.skill;
            GameObject card = Instantiate(spellCardPref, cardParent);
            card.transform.Find("Deactive/Button").GetComponent<Button>().onClick.AddListener(
                () => CancelCoolTimeBtnClicked(card));

            card.transform.Find("Name/Value").GetComponent<Text>().text = skill.name;
            ActiveCardInfo activeCardInfo = card.AddComponent<ActiveCardInfo>();
            activeCardInfo.data = spellCard;

            card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "spell", skill.name);

            if (skill.cost.food > 0) card.transform.Find("Cost/FoodIcon/Value").GetComponent<Text>().text = skill.cost.food.ToString();
            if (skill.cost.gold > 0) card.transform.Find("Cost/GoldIcon/Value").GetComponent<Text>().text = skill.cost.gold.ToString();
            card.transform.Find("Tier/Value").GetComponent<Text>().text = skill.tierNeed.ToString();

            card.AddComponent<Index>().Id = index;
            origin.Add(card);
            Deck.Add(index);
            if (skill.method.methodName != "skill_magma" && skill.method.methodName != "skill_unit_heal") {
                DeactiveCard(activeCardInfo.data.parentBuilding);
            }
            else if(skill.method.methodName == "skill_unit_heal") {
                card.AddComponent<HealArea>();
                Destroy(card.GetComponent<IngameDragHandler>());
            }
                
            card.SetActive(false);
            index++;
        }

        for(int i=0; i<HAND_MAX_COUNT; i++) {
            DrawCard();
        }
    }

    //카드 뽑기
    public void DrawCard() {
        List<int> pool = new List<int>();
        foreach (int item in Deck) {
            ActiveCardInfo info = origin[item].GetComponent<ActiveCardInfo>();
            if (CanUseCard(info)) pool.Add(origin[item].GetComponent<Index>().Id);
        }
        if (pool.Count == 0) return;

        int selectedIndex = rand.Next(0, pool.Count);
        Hand.Add(pool[selectedIndex]);
        Debug.Log(pool[selectedIndex]);
        origin[pool[selectedIndex]].SetActive(true);
        origin[pool[selectedIndex]].transform.SetAsFirstSibling();
        Deck.Remove(pool[selectedIndex]);
    }

    //card use
    public void UseCard(GameObject selectedObject) {
        int id = selectedObject.GetComponent<Index>().Id;
        //Debug.Log("ID : " + id);

        var match = origin.Find(x => id == x.GetComponent<Index>().Id);
        if (match == null) return;

        match.SetActive(false);
        Hand.Remove(id);

        ActiveCard activeCard = selectedObject.GetComponent<ActiveCardInfo>().data;
        //spell은 쿨타임
        //유닛은 핸드, 덱에서 제거
        if (!string.IsNullOrEmpty(activeCard.baseSpec.skill.name)) {
            Deck.Add(id);

            ActiveCardCoolTime cooltimeComp = activeCard.parentBuilding.AddComponent<ActiveCardCoolTime>();
            cooltimeComp.cards = origin;
            cooltimeComp.coolTime = activeCard.baseSpec.skill.coolTime;
            cooltimeComp.StartCool();
        }
        DrawCard();
    }

    private bool CanUseCard(ActiveCardInfo data) {
        Cost cost = data.data.baseSpec.unit.cost;
        Unit unit = data.data.baseSpec.unit;
        Skill skill = data.data.baseSpec.skill;

        if (!string.IsNullOrEmpty(unit.name)) {
            if (playerController.hqLevel >= unit.tierNeed) return true;
            else return false;
        }
        else {
            if ((!string.IsNullOrEmpty(skill.name))) {
                if (playerController.hqLevel >= skill.tierNeed) return true;
                else return false;
            }
        }
        return false;
    }

    //(핸드)카드 교체 기능
    //쿨타임 30초
    public void HandReset() {
        //쿨타임이면 return
        IngameHandChangeCoolTime prevComp = playerController.gameObject.GetComponent<IngameHandChangeCoolTime>();
        if (prevComp != null) return;

        foreach (int index in Hand) {
            Deck.Add(index);
            origin[index].SetActive(false);
        }
        Hand.Clear();

        playerController.resourceClass.turn--;
        playerController.PrintResource();

        IngameHandChangeCoolTime coolComp = playerController.gameObject.AddComponent<IngameHandChangeCoolTime>();
        coolComp.coolTime = 30;
        coolComp.Btn = refreshCardBtn;
        coolComp.StartCool();

        for (int i = 0; i < HAND_MAX_COUNT; i++) {
            DrawCard();
        }
    }

    /// <summary>
    /// 쿨타임 제거 버튼
    /// </summary>
    public void CancelCoolTimeBtnClicked(GameObject card) {
        ActiveCardInfo cardInfo = card.GetComponent<ActiveCardInfo>();
        int tier = cardInfo.data.baseSpec.unit.tierNeed;
        int lv = cardInfo.data.ev.lv;
        ActiveCardCoolTime coolTime = cardInfo.data.parentBuilding.GetComponent<ActiveCardCoolTime>();
        if (coolTime == null) return;
        uint cost = coolTime.cancelCooltimeCost;
        if (cost > playerController.Gold) return;
        if (cost > playerController.Food) return;

        playerController.Gold -= cost;
        playerController.Food -= cost;
        playerController.PrintResource();

        coolTime.OnTime();
    }
}
