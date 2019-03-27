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
    [SerializeField] Transform cardParent;
    [SerializeField] GameObject refreshCardBtn;

    private static int HAND_MAX_COUNT = 5;
    private readonly System.Random rand = new System.Random((int)DateTime.Now.Ticks);

    [SerializeField] public List<GameObject> origin = new List<GameObject>();    //원본 액티브 카드 리스트
    [SerializeField] List<int> Deck = new List<int>();  //덱 인덱스 리스트
    [SerializeField] List<int> Hand = new List<int>();  //핸드 인덱스 리스트
    [SerializeField] List<int> Grave = new List<int>();   //Draw발동시 사용된 카드가 임시로 머무는 장소
    void Awake() {
        ingameCityManager = GetComponent<IngameCityManager>();
        eventHandler = IngameSceneEventHandler.Instance;
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.UNIT_UPGRADED, OnUnitUpgraded);
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, OnHqUpgraded);

        playerController = PlayerController.Instance;
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
        //GameObject card = origin.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == parentBuilding);
        //IngameCityManager.BuildingInfo buildingInfos = ingameCityManager.myBuildingsInfo.Find(x => x.tileNum == parentBuilding.GetComponent<BuildingObject>().setTileLocation);
        

        //int index = card.GetComponent<Index>().Id;
        //buildingInfos.activate = true;
        //buildingInfos.gameObject.GetComponent<TileSpineAnimation>().SetUnit(true);
        //if (isDead) {
        //    ActiveCardCoolTime comp = parentBuilding.AddComponent<ActiveCardCoolTime>();
        //    comp.coolTime = CalculateHeroCoolTime(card.GetComponent<ActiveCardInfo>());
        //    comp.cards = origin;            
        //    comp.StartCool();
        //}

        //if (Hand.Count == HAND_MAX_COUNT) {
        //    Deck.Add(index);
        //}
        //else {
        //    Hand.Add(index);
        //    origin[index].SetActive(true);
        //    origin[index].transform.SetAsLastSibling();
        //}
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

        //if(card.GetComponent<IngameDragHandler>() == null) {
        //    if (!string.IsNullOrEmpty(skill.name)) {
        //        if (skill.method.methodName == "skill_unit_heal") {
        //            card.GetComponent<HealArea>().CancelDrag();
        //            card.GetComponent<HealArea>().enabled = false;
        //        }
        //    }
        //}
        //else {
        //    card.GetComponent<IngameDragHandler>().CancelDrag();
        //    card.GetComponent<IngameDragHandler>().enabled = false;
        //    card.transform.Find("Deactive").gameObject.SetActive(true);
        //}
        card.transform.Find("Deactive/Button").gameObject.SetActive(false);

        LayoutRebuilder.ForceRebuildLayoutImmediate(cardParent.GetComponent<RectTransform>());
        card.SetActive(false);
    }

    public void ActivateCard(GameObject parentBuilding) {
        GameObject card = origin.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == parentBuilding);
        if (card == null) return;

        card.SetActive(true);
        Skill skill = card.GetComponent<ActiveCardInfo>().data.baseSpec.skill;
        //if (!string.IsNullOrEmpty(skill.name)) {
        //    if (skill.method.methodName != "skill_magma" && skill.method.methodName != "skill_unit_heal") {
        //        card.GetComponent<IngameDragHandler>().enabled = false;
        //        card.transform.Find("Deactive").gameObject.SetActive(true);
        //        card.transform.Find("Deactive/Button").gameObject.SetActive(false);
        //    }
        //    else if(skill.method.methodName == "skill_unit_heal") {
        //        card.GetComponent<HealArea>().enabled = true;
        //        card.transform.Find("Deactive").gameObject.SetActive(false);
        //        card.transform.Find("Deactive/Button").gameObject.SetActive(true);
        //    }
        //}
        //else {
        //    card.GetComponent<IngameDragHandler>().enabled = true;
        //    card.transform.Find("Deactive").gameObject.SetActive(false);
        //    card.transform.Find("Deactive/Button").gameObject.SetActive(true);
        //}
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardParent.GetComponent<RectTransform>());
    }

    public void MakeMagmaCard(int num) {
        for(int i=0; i<num; i++) {
            Skill skill = new Skill();
            skill.method = new SkillDetail() { methodName = "skill_magma" };
            GameObject card = Instantiate(spellCardPref, cardParent);
            AddSkill(skill.method.methodName, card);
        }
    }

    public void InitCard() {
        foreach(GameObject card in origin) {
            Destroy(card);
        }
        origin.Clear();
        Deck.Clear();
        Hand.Clear();

        int index = 0;

        foreach (ActiveCard unitCard in playerController.playerActiveCards().unitCards()) {
            Unit unit = unitCard.baseSpec.unit;
            GameObject card = Instantiate(unitCardPref, cardParent);
            card.transform.Find("Deactive/Button").GetComponent<Button>().onClick.AddListener(
                () => CancelCoolTimeBtnClicked(card));

            card.transform.Find("Name/Value").GetComponent<Text>().text = unit.name;
            ActiveCardInfo activeCardInfo = card.AddComponent<ActiveCardInfo>();
            activeCardInfo.data = unitCard;
            card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "unit", unit.name);

            //if (unit.cost.food > 0) card.transform.Find("Cost/FoodIcon/Value").GetComponent<Text>().text = unit.cost.food.ToString();
            if (unit.cost.gold > 0) card.transform.Find("Cost/GoldIcon/Value").GetComponent<Text>().text = unit.cost.gold.ToString();
            //card.transform.Find("Tier/Value").GetComponent<Text>().text = unit.tierNeed.ToString();

            card.AddComponent<Index>().Id = index;
            card.SetActive(false);
            origin.Add(card);
            Deck.Add(index);
            index++;
        }

        foreach (ActiveCard spellCard in playerController.playerActiveCards().spellCards()) {
            Skill skill = spellCard.baseSpec.skill;
            GameObject card = Instantiate(spellCardPref, cardParent);
            card.transform.Find("Deactive/Button").GetComponent<Button>().onClick.AddListener(
                () => CancelCoolTimeBtnClicked(card));

            card.transform.Find("Name/Value").GetComponent<Text>().text = skill.name;
            ActiveCardInfo activeCardInfo = card.AddComponent<ActiveCardInfo>();
            activeCardInfo.data = spellCard;

            card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "spell", skill.name);

            //if (skill.cost.food > 0) card.transform.Find("Cost/FoodIcon/Value").GetComponent<Text>().text = skill.cost.food.ToString();
            if (skill.cost.gold > 0) card.transform.Find("Cost/GoldIcon/Value").GetComponent<Text>().text = skill.cost.gold.ToString();
            card.transform.Find("Tier/Value").GetComponent<Text>().text = skill.tierNeed.ToString();

            card.AddComponent<Index>().Id = index;
            origin.Add(card);
            Deck.Add(index);

            AddSkill(skill.method.methodName, card);
            card.SetActive(false);
            index++;
        }

        for(int i=0; i<HAND_MAX_COUNT; i++) {
            DrawCard();
        }
    }

    public void DrawCard(int prevId) {
        if (Deck.Count == 0) {
            if(Grave.Count == 1 && Grave[0] == prevId) {
                Deck.Add(prevId);
                Grave.Remove(prevId);
            }
            
            if(Grave.Count >= 2) {
                int index = Grave.Find(x => x != prevId);
                Deck.Add(index);
                Grave.Remove(index);
            }
        }
        if (Deck.Count == 0) return;

        int selectedIndex = rand.Next(0, Deck.Count);
        Hand.Add(Deck[selectedIndex]);
        Debug.Log(Deck[selectedIndex]);
        origin[Deck[selectedIndex]].SetActive(true);
        origin[Deck[selectedIndex]].transform.SetAsFirstSibling();
        Deck.Remove(Deck[selectedIndex]);
    }

    //카드 뽑기
    public void DrawCard() {
        if (Deck.Count == 0) {
            Grave.AddRange(Grave);
            Grave.Clear();
        }
        if (Deck.Count == 0) return;

        int selectedIndex = rand.Next(0, Deck.Count);
        Hand.Add(Deck[selectedIndex]);
        Debug.Log(Deck[selectedIndex]);
        origin[Deck[selectedIndex]].SetActive(true);
        origin[Deck[selectedIndex]].transform.SetAsFirstSibling();
        Deck.Remove(Deck[selectedIndex]);
    }

    //card use
    public void UseCard(GameObject selectedObject) {
        int id = selectedObject.GetComponent<Index>().Id;
        //Debug.Log("ID : " + id);

        var match = origin.Find(x => id == x.GetComponent<Index>().Id);
        if (match == null) return;

        ActiveCardInfo activeCard = selectedObject.GetComponent<ActiveCardInfo>();
        if (CanUseCard(activeCard)) {
            match.SetActive(false);
            Hand.Remove(id);

            Grave.Add(id);
            DrawCard(id);

            switch (activeCard.data.type) {
                case "unit":
                    playerController.playerResource().UseGold(activeCard.data.baseSpec.unit.cost.gold);
                    break;
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

        //playerController.PrintResource();

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
        //int tier = cardInfo.data.baseSpec.unit.tierNeed;
        int lv = cardInfo.data.ev.lv;
        ActiveCardCoolTime coolTime = cardInfo.data.parentBuilding.GetComponent<ActiveCardCoolTime>();
        if (coolTime == null) return;
        uint cost = coolTime.cancelCooltimeCost;
        //if (cost > playerController.Gold) return;
        //if (cost > playerController.Food) return;

        //playerController.Gold -= cost;
        //playerController.Food -= cost;
        //playerController.PrintResource();

        coolTime.OnTime();
    }
}


public partial class IngameDeckShuffler : SerializedMonoBehaviour {
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<Effects, GameObject> effectModules;
    public Camera camera;

    private void AddSkill(string methodName, GameObject card) {
        switch (methodName) {
            case "magma":
                Destroy(card.GetComponent<IngameDragHandler>());
                MagmaDragHandler magmaDragHandler = card.AddComponent<MagmaDragHandler>();

                magmaDragHandler.Init(
                    camera,
                    effectModules[Effects.skill_magma],
                    PlayerController.Instance.maps[PlayerController.Player.PLAYER_1].transform.parent,
                    card.GetComponent<ActiveCardInfo>().data.parentBuilding,
                    this
                );
                //effects[Effects.skill_magma]
                break;
        }
    }

    public enum Effects {
        skill_magma
    }
}