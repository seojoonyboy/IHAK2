using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DataModules;
using System;
using UnityEngine.Events;

public partial class HeroAI : UnitAI {
    public enum skillState {
        COOLING,
        SKILLING,
        WAITING_DONE
    }

	private Transform expBar;
    private TextMeshPro LvText;
	private decimal attackSP;
	[SerializeField] private ActiveCard unitCard;
    private List<HeroAI> fightHeroes;


    private timeUpdate skillUpdate;
    private float coolTime;
    private float activeSkillCoolTime = 1f; //TODO : 임시
    private float skillUsingTime = 1f;  // TODO : 임시
    
    private UnityAction skillActivate;
    
    private void setState(skillState state) {
        skillUpdate = null;
        coolTime = 0f;
        switch (state) {
            case skillState.COOLING:
                skillUpdate = CoolTimeUpdate;
                break;
            case skillState.SKILLING:
                skillUpdate = UsingSkillUpdate;
                break;
            case skillState.WAITING_DONE:
                skillUpdate = noneUpdate;
                break;
        }
    }

    void Update() {
        update(Time.deltaTime);
        skillUpdate(Time.deltaTime);
    }

    void CoolTimeUpdate(float time) {
        coolTime += time;
        if(coolTime < activeSkillCoolTime) return;
        Debug.Log("스킬 발동");
        skillActivate();
        setState(skillState.SKILLING);

    }

    void UsingSkillUpdate(float time) {
        coolTime += time;
        if(coolTime < activeSkillCoolTime) return;
        setState(skillState.COOLING);

    }

    void noneUpdate(float time) {}

    private void FindUnitSkill(UnitSkill unitSkill) {
        if(unitSkill == null) {
            setState(skillState.WAITING_DONE);
            return;
        }
        switch(unitSkill.method.methodName) {
            case "bite" : //라칸 물어뜯기
            skillActivate = Lakan_bite;
            break;
            case "arsonist" :   //쉘 방화범
            skillActivate = Shell_humantorch;
            break;
            //case ""
            default :
            skillActivate = (() => noneUpdate(0f));
            setState(skillState.WAITING_DONE);
            break;
        }
    }

	private void Init() {
        if (healthBar != null) return;
        healthBar = transform.Find("UnitBar/HP");
        expBar = transform.Find("UnitBar/Exp");
        LvText = transform.Find("UnitBar/LevelBackGround/Level").GetComponent<TextMeshPro>();
        unitSpine = GetComponentInChildren<UnitSpine>();
        fightHeroes = new List<HeroAI>();
		InitStatic();
    }

    public override void SetUnitData(ActiveCard card) {
        Init();
        this.unitCard = card;
        Unit unit = card.baseSpec.unit;
        moveSpeed = unit.moveSpeed * 0.4f;
		attackSpeed = unit.attackSpeed;
		attackRange = unit.attackRange;
		attackSP = unit.attackSP;
        power = unit.power;
        if(card.ev.lv <= 0) card.ev.lv = 1;
        SetMaxHP();
        if(health == 0) health = card.ev.hp;
        if(health == 0) health = maxHealth;
        else health += HealTime();
        if(health > maxHealth) health = maxHealth;
        calculateHealthBar();
        calculateExpBar();
        ChangeLvText();
        setState(skillState.COOLING);
        FindUnitSkill(unit.skill);
    }

    public override void damaged(float damage) {
        base.damaged(damage);
        unitCard.TakeDamage(Mathf.RoundToInt(damage));
    }

    public override void SetUnitData(Unit unit, int level) {
        Init();
        unitCard = new ActiveCard();
        unitCard.baseSpec.unit = unit;
        moveSpeed = unit.moveSpeed * 0.1f;
        attackSpeed = unit.attackSpeed;
        attackRange = unit.attackRange;
        attackSP = unit.attackSP;
        power = unit.power;
        power = PowerUP(power);
        unitCard.ev = new Ev() { lv = level };
        SetMaxHP();
        if(health == 0) health = unitCard.ev.hp;
        if(health == 0) health = maxHealth;
        else health += HealTime();
        if(health > maxHealth) health = maxHealth;
        calculateHealthBar();
        calculateExpBar();
        ChangeLvText();
        setState(skillState.COOLING);
        FindUnitSkill(unit.skill);
	}

    public override void ResetSpeedPercentage() {
        moveSpeed = unitCard.baseSpec.unit.moveSpeed;
    }

    public void ExpGain(int exp) {
        unitCard.ev.exp += exp;
        CheckLv();
        calculateExpBar();
    }

    private float ExpNeed() {
        return unitCard.ev.lv * 1.5f * 100;
    }

    private void CheckLv() {
        bool isLvUp = ExpNeed() <= unitCard.ev.exp;
        if(isLvUp) ChangeStat();
    }

    private void ChangeStat() {
        if(unitCard.ev.lv >= 10) return;
        unitCard.ev.lv++;
        unitCard.ev.exp = 0;
        power = PowerUP(power);
        LvUpHP();
        ChangeLvText();
    }

    private void ChangeLvText() {
        LvText.text = unitCard.ev.lv.ToString();
    }

    private int PowerUP(float stat) {
        return Mathf.RoundToInt(((100f + unitCard.ev.lv * 15f) / 100f) * stat);
    }

    public override int CalPower() {
        return Mathf.RoundToInt(power);
    }

    private float HealTime() {
        int totalTime = (int)Time.realtimeSinceStartup - unitCard.ev.time;
        float healed = maxHealth * totalTime * 0.03f; 
        return healed;
    }

    private void LvUpHP() { //레벨업 했을 때 최대체력 변화와 그에 따른 체력 추가를 보는것.
        float beforeMax = maxHealth;
        SetMaxHP();
        beforeMax = maxHealth - beforeMax;
        health += beforeMax;
    }
    
	private void calculateExpBar() {
        float percent = (float)unitCard.ev.exp / ExpNeed();
        expBar.transform.localScale = new Vector3(percent, 1f, 1f);
    }

	private void SetMaxHP() {
        maxHealth = PowerUP((float)unitCard.baseSpec.unit.hitPoint);
    }

    public override void DestoryEnemy() {
        TileReset();
        unitCard.ChangeHp(0);

        if(gameObject.layer == myLayer) {
            ingameDeckShuffler.HeroReturn(unitCard.parentBuilding, true);
        }
        else if(gameObject.layer == enemyLayer) {
            enemyHeroGenerator.HeroReturn(unitCard.baseSpec.unit.id);
        }
        GiveExp();
        Destroy(gameObject);
    }

	public override void ReturnDeck(Enum Event_Type, Component Sender, object Param) {
        unitCard.ChangeHp((int)health);
        unitCard.ev.time = (int)Time.realtimeSinceStartup;
        ingameDeckShuffler.HeroReturn(unitCard.parentBuilding, false);
        Destroy(gameObject);
    }

    public override void attackingHero(UnitAI unit) {
        if(unit.GetComponent<HeroAI>() == null) return;
        for (int i = 0; i < fightHeroes.Count; i++)
            if(fightHeroes[i].gameObject == null || fightHeroes[i].gameObject == unit.gameObject) return;
        fightHeroes.Add(unit.GetComponent<HeroAI>());
    }

    private void GiveExp() {
        if(fightHeroes.Count == 0) return;
        int exp = Mathf.FloorToInt(200f * unitCard.ev.lv * unitCard.baseSpec.unit.id.CompareTo("n_uu_02002") == 0 ? 2 : 1 / 5f);
        for(int i = 0; i < fightHeroes.Count; i++) {
            if(fightHeroes[i] == null) {
                fightHeroes.RemoveAt(i);
                i--;
            }
        }
        if(fightHeroes.Count == 0) return;
        exp /= fightHeroes.Count;
        foreach(HeroAI hero in fightHeroes) hero.ExpGain(exp);
    }
}
