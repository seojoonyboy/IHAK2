using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DataModules;
using System;
using UnityEngine.UI;

public partial class HeroAI : UnitAI {
    public GameObject targetCard;

    private Transform expBar;
    private Transform cooltimeBar;
    private TextMeshPro LvText;
    private decimal attackSP;
    [SerializeField] public ActiveCard unitCard;
    private List<HeroAI> fightHeroes;
    IEnumerator coroutine;

    public override void Init(object card) {
        healthBar = transform.Find("UnitBar/HP");
        expBar = transform.Find("UnitBar/Exp");
        cooltimeBar = transform.Find("UnitBar/SkillCool");
        LvText = transform.Find("UnitBar/LevelBackGround/Level").GetComponent<TextMeshPro>();
        unitSpine = GetComponentInChildren<UnitSpine>();
        fightHeroes = new List<HeroAI>();
        InitStatic();
    }

    public override void Init(object card, GameObject cardObj) {
        Init(card);
        ActiveCard actcard = (ActiveCard)card;
        MaxHealth = actcard.baseSpec.unit.hitPoint;
        HP = MaxHealth;
        this.unitCard = actcard;
        Unit unit = actcard.baseSpec.unit;
        int level = (actcard.ev.lv <= 0) ? 1 : actcard.ev.lv;
        SetUnitDataCommon(level);
        SetColliderData();
        if(cardObj == null) return;
        unitCard.gameObject = cardObj;
        coroutine = UpdateInfoCard();
        StartCoroutine(coroutine);
    }

    //public override void setunitdata(unit unit, int level) {
    //    init(unit);
    //    unitcard = new activecard();
    //    unitcard.basespec.unit = unit;
    //    setunitdatacommon(level);
    //    setcolliderdata();
    //}

    private void SetUnitDataCommon(int level) {
        Unit unit = unitCard.baseSpec.unit;
        moveSpeed = unit.moveSpeed;
        attackSpeed = unit.attackSpeed;
        attackRange = unit.attackRange;
        power = unit.attackPower;
        power = PowerUP(power);
        unitCard.ev = new Ev() { lv = level };
        SetMaxHP();
        if (HP == 0) HP = MaxHealth = unitCard.ev.hp;
        else HP += HealTime();
        if (HP > MaxHealth) HP = MaxHealth;
        calculateExpBar();
        ChangeLvText();
        setState(skillState.COOLING);
        FindUnitSkill(unit.skill);
    }

    IEnumerator UpdateInfoCard() {
        while (true) {
            if(unitCard.gameObject != null) {
                Slider healthSlider = unitCard.gameObject.transform.Find("Stats/Health").GetComponent<Slider>();
                Slider expSlider = unitCard.gameObject.transform.Find("Stats/Exp").GetComponent<Slider>();

                healthSlider.value = HP;
                healthSlider.maxValue = MaxHealth;

                expSlider.value = unitCard.ev.exp;
                expSlider.maxValue = ExpNeed();

                //Debug.Log("HP : " + health);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    void OnDestroy() {
        if (standingStation != null) standingStation.DestroyEnteredTarget(gameObject);
        if (coroutine != null) StopCoroutine(coroutine);
    }

    public override void attackUnit() {
        base.attackUnit();
        if(weaponSkill != null) weaponSkill();
    }

    public override void Damage(float damage) {
        base.Damage(damage);
        unitCard.TakeDamage(Mathf.RoundToInt(damage));
    }

    public override void Damage(float damage, Transform enemy) {
        base.Damage(damage, enemy);
        UnitAI unitAI = enemy.GetComponent<UnitAI>();
        if(unitAI == null) return;
        attackingHero(unitAI);
    }

    public void ExpGain(int exp) {
        ConditionSet expSet = playerController.MissionConditionsController().conditions.Find(x => x.condition == Conditions.exp_add);
        if (expSet != null) {
            int percentage = expSet.args[0];
            exp = 0;
            //Debug.Log("영웅 경험치 보정");
        }
        unitCard.ev.exp += exp;
        CheckLv();
        calculateExpBar();
    }

    private float ExpNeed() {
        return unitCard.ev.lv * 1.5f * 100;
    }

    private void CheckLv() {
        bool isLvUp = ExpNeed() <= unitCard.ev.exp;
        if (!isLvUp) return;
        ChangeStat();

        if(unitCard.gameObject == null) return;
        Text cardLvText = unitCard.gameObject.transform.Find("Lv").GetComponent<Text>();
        cardLvText.text = "Lv. " + unitCard.ev.lv;

        Text atk = unitCard.gameObject.transform.Find("Specs/Base/Atk/Value").GetComponent<Text>();
        atk.text = "+ " + power;
    }

    private void ChangeStat() {
        if (unitCard.ev.lv >= 10) return;
        unitCard.ev.lv++;
        unitCard.ev.exp = 0;
        power = PowerUP(power);
        LvUpMinions();
        LvUpHP();
        ChangeLvText();
        if (unitCard.ev.lv == 2) eventHandler.PostNotification(IngameSceneEventHandler.MISSION_EVENT.UNIT_LEVEL_UP, null, null);        
    }

    private void ChangeLvText() {
        LvText.text = unitCard.ev.lv.ToString();
    }

    private int PowerUP(float stat) {
        return Mathf.RoundToInt(((100f + unitCard.ev.lv * 15f) / 100f) * stat);
    }

    private void LvUpMinions() {
        Transform heroGroup = transform.parent;
        for (int i = 3; i < heroGroup.childCount; i++) {
            heroGroup.GetChild(i).GetComponent<MinionAI>().SetMinionData(unitCard, true);
        }
    }

    public override int CalPower() {
        return Mathf.RoundToInt(power);
    }

    private float HealTime() {
        int totalTime = (int)Time.realtimeSinceStartup - unitCard.ev.time;
        float healed = MaxHealth * totalTime * 0.03f;
        return healed;
    }

    private void LvUpHP() { //레벨업 했을 때 최대체력 변화와 그에 따른 체력 추가를 보는것.
        float beforeMax = MaxHealth;
        SetMaxHP();
        beforeMax = MaxHealth - beforeMax;
        HP += beforeMax;
    }

    private void calculateExpBar() {
        float percent = (float)unitCard.ev.exp / ExpNeed();
        expBar.transform.localScale = new Vector3(percent, 1f, 1f);
    }

    private void SetMaxHP() {
        MaxHealth = PowerUP((float)unitCard.baseSpec.unit.hitPoint);
    }

    public override void Die() {
        unitCard.ChangeHp(0);

        if (gameObject.layer == myLayer) {
            ingameDeckShuffler.HeroReturn(targetCard, true);
        }
        else if (gameObject.layer == enemyLayer) {
            //TODO : 적 사망시 저장 장소 세팅 필요
            //enemyHeroGenerator.HeroReturn(unitCard.baseSpec.unit.id);
        }
        GiveExp();
        Destroy(gameObject);
        myGroup.UnitDead(gameObject);
    }

    public override void ReturnDeck(Enum Event_Type, Component Sender, object Param) {
        unitCard.ChangeHp((int)HP);
        unitCard.ev.time = (int)Time.realtimeSinceStartup;
        ingameDeckShuffler.HeroReturn(targetCard, false);
        Destroy(gameObject);
    }

    public override void attackingHero(UnitAI unit) {
        HeroAI heroAI = unit.GetMyHeroAI();
        if(heroAI == null) return;
        for (int i = 0; i < fightHeroes.Count; i++)
            if (fightHeroes[i].gameObject == null || fightHeroes[i].gameObject == unit.gameObject) return;
        fightHeroes.Add(heroAI);
    }

    public override HeroAI GetMyHeroAI() {
        return this;
    }

    protected override void GiveExp() {
        if (fightHeroes.Count == 0) return;
        int exp = Mathf.FloorToInt(200f * unitCard.ev.lv * unitCard.baseSpec.unit.id.CompareTo("n_uu_02002") == 0 ? 2 : 1 / 5f);
        RemoveDeadHeroNoExp();
        if (fightHeroes.Count == 0) return;
        exp /= fightHeroes.Count;
        foreach (HeroAI hero in fightHeroes) hero.ExpGain(exp);
    }

    private void RemoveDeadHeroNoExp() {
        for (int i = 0; i < fightHeroes.Count; i++) {
            if (fightHeroes[i] == null) {
                fightHeroes.RemoveAt(i);
                i--;
            }
        }
    }

    public override void ResetStat() {
        Unit unit = unitCard.baseSpec.unit;
        moveSpeed = unit.moveSpeed;
        attackSpeed = unit.attackSpeed;
        attackRange = unit.attackRange;
        power = PowerUP(unit.attackPower);
        MaxHealth = PowerUP(unit.hitPoint);
    }

    public MapStation GetCurrentNode() {
        UnitGroup group = GetComponentInParent<UnitGroup>();
        return group.currentStation;
    }
}
