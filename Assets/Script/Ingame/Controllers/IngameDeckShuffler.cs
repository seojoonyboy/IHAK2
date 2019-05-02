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
        eventHandler = IngameSceneEventHandler.Instance;
        playerController = PlayerController.Instance;
    }

    void OnDestroy() {
    }

    public void HeroReturn(GameObject targetCard, bool isDead) {
        GameObject card = heroCards.Find(
            x => x == targetCard
        );

        if(card == null) {
            Debug.LogError("사망한 영웅 유닛의 카드를 찾지 못하였습니다!");
            return;
        }

        int index = card.GetComponent<Index>().Id;
        //buildingInfos.activate = true;
        //buildingInfos.gameObject.GetComponent<TileSpineAnimation>().SetUnit(true);
        if (isDead) {

            if (card.GetComponent<ActiveCardCoolTime>() == null) {
                ActiveCardCoolTime comp = card.AddComponent<ActiveCardCoolTime>();
                comp.targetCard = card;
                comp.coolTime = CalculateHeroCoolTime(card.GetComponent<ActiveCardInfo>());
                comp.StartCool();
            }
        }
    }

    private float CalculateHeroCoolTime(ActiveCardInfo card) {
        float baseCool = card.data.baseSpec.unit.coolTime;

        ConditionSet expSet = playerController
            .MissionConditionsController()
            .conditions.Find(x => x.condition == Conditions.cooltime_fix || x.condition == Conditions.hero_cooltime_fix);
        if(expSet != null) baseCool = expSet.args[0];

        return baseCool;
    }

    public void HeroReturnBtnClicked() {
        eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, this);
    }

    public void InitCard() {
        heroCards = new List<GameObject>();
        spellCards = new List<GameObject>();

        int index = 0;
        foreach (ActiveCard unitCard in playerController.playerActiveCards().unitCards()) {
            Unit unit = unitCard.baseSpec.unit;
            GameObject card = Instantiate(unitCardPref, heroCardParent);
            card.GetComponent<Toggle>().group = card.transform.parent.parent.GetComponent<ToggleGroup>();

            ActiveCardInfo activeCardInfo = card.AddComponent<ActiveCardInfo>();
            activeCardInfo.data = unitCard;
            card.transform.Find("InnerBg/Portrait").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "unit", unit.id);

            int cost = (int)unit.cost.gold;
            card.transform.Find("Cost/Value").GetComponent<Text>().text = cost.ToString();

            Slider healthSlider = card.transform.Find("Health").GetComponent<Slider>();
            healthSlider.value = healthSlider.maxValue = unit.hitPoint;

            card.transform.Find("MinionInfo/DivideIcon/MaxMinion").GetComponent<Text>().text = unit.minion.count.ToString();
            card.transform.Find("Type").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetTypeImage(null, unit.attackType);

            heroCards.Add(card);

            card.AddComponent<Index>().Id = index;
            index++;
        }

        index = 0;
        foreach (ActiveCard spellCard in playerController.playerActiveCards().spellCards()) {
            ActiveSkill skill = spellCard.baseSpec.skill;
            GameObject card = Instantiate(spellCardPref, spellCardParent);

            card.GetComponent<Toggle>().group = card.transform.parent.parent.GetComponent<ToggleGroup>();

            card.transform.Find("Name").GetComponent<Text>().text = skill.name;
            ActiveCardInfo activeCardInfo = card.AddComponent<ActiveCardInfo>();
            activeCardInfo.data = spellCard;

            card.transform.Find("Image").GetComponent<Image>().sprite = ConstructManager.Instance.GetComponent<CardImages>().GetImage("primal", "spell", skill.name);

            //if (skill.cost.food > 0) card.transform.Find("Cost/FoodIcon/Value").GetComponent<Text>().text = skill.cost.food.ToString();
            if (skill.cost.gold > 0) card.transform.Find("Cost/GoldIcon/Value").GetComponent<Text>().text = skill.cost.gold.ToString();

            card.AddComponent<Index>().Id = index;

            spellCards.Add(card);

            AddSkill(skill.method.methodName, card, skill.method.args, skill.coolTime);
            index++;
        }

        spellCards = SpellCardSort();
        foreach(GameObject card in spellCards) {
            //ActiveCardInfo info = card.GetComponent<ActiveCardInfo>();
            //Debug.Log(info.data.baseSpec.skill.cost.gold);
            card.transform.SetAsLastSibling();
        }
    }

    //card use
    public bool UseCard(GameObject selectedObject) {
        int id = selectedObject.GetComponent<Index>().Id;
        ActiveCardInfo activeCard = selectedObject.GetComponent<ActiveCardInfo>();
        if (CanUseCard(activeCard)) {
            switch (activeCard.data.type) {
                //영웅 유닛 카드는 사용시 아예 핸드, 덱에서 제외
                case "hero":
                    playerController.playerResource().UseGold(activeCard.data.baseSpec.unit.cost.gold);
                    var effectModules = playerController.PlayerPassiveCards().effectModules;
                    if (effectModules.ContainsKey("Unit_health")) {
                        activeCard.data.baseSpec.unit.hitPoint += Mathf.RoundToInt(effectModules["Unit_health"]);
                    }
                    playerController.HeroSummon(activeCard.data, selectedObject);

                    selectedObject.transform.Find("Cost").gameObject.SetActive(false);
                    break;
                //마법 주문 카드는 사용시 다시 덱에 들어감.
                case "active":
                    playerController.playerResource().UseGold(activeCard.data.baseSpec.skill.cost.gold);
                    break;
            }
            return true;
        }
        else {
            IngameAlarm.instance.SetAlarm("자원이 부족합니다!");
            return false;
        }
    }

    public bool CanUseCard(ActiveCardInfo card) {
        string type = card.data.type;
        Cost cost = null;
        switch (type) {
            case "hero":
                cost = new Cost();
                cost.gold = card.data.baseSpec.unit.cost.gold * 100;
                cost.population = card.data.baseSpec.unit.cost.population;
                break;
            case "active":
                cost = new Cost();
                cost.gold = card.data.baseSpec.skill.cost.gold * 100;
                cost.population = card.data.baseSpec.skill.cost.population;
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
        //ActiveCardInfo cardInfo = card.GetComponent<ActiveCardInfo>();
        //int lv = cardInfo.data.ev.lv;
        //ActiveCardCoolTime coolTime = cardInfo.data.parentBuilding.GetComponent<ActiveCardCoolTime>();
        //if (coolTime == null) return;
        //uint cost = coolTime.cancelCooltimeCost;

        //coolTime.OnTime();
    }

    private List<GameObject> SpellCardSort() {
        spellCards.Sort(delegate (GameObject x, GameObject y) {
            if (x.GetComponent<ActiveCardInfo>().data.baseSpec.skill.cost.gold == y.GetComponent<ActiveCardInfo>().data.baseSpec.skill.cost.gold) return 0;
            else if (x.GetComponent<ActiveCardInfo>().data.baseSpec.skill.cost.gold > y.GetComponent<ActiveCardInfo>().data.baseSpec.skill.cost.gold) return 1;
            else return -1;
        });
        return spellCards;
    }
}


public partial class IngameDeckShuffler : SerializedMonoBehaviour {
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<Effects, GameObject> effectModules;
    public Camera camera;

    private void AddSkill(string methodName, GameObject card, string[] args, int coolTime) {
        switch (methodName) {
            case "magma":
                card.GetComponent<SpellCardHandler>().prefab = effectModules[Effects.skill_magma];
                break;

            case "herb_distribution":
                card.GetComponent<SpellCardHandler>().prefab = effectModules[Effects.skill_herb];
                break;
            case "scary_prediction":
                card.GetComponent<SpellCardHandler>().prefab = effectModules[Effects.skill_scaryOracle];
                break;

            case "war_cry":
                card.GetComponent<SpellCardHandler>().prefab = effectModules[Effects.skill_warcry];
                break;
        }
        card.GetComponent<SpellCardHandler>().data = args;
        card.GetComponent<SpellCardHandler>().coolTime = coolTime;
        card.GetComponent<SpellCardHandler>().targetCard = card;
    }

    public enum Effects {
        skill_magma,
        skill_herb,
        skill_scaryOracle,
        skill_warcry
    }
}