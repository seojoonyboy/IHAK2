using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DataModules;
using System;
using UnityEngine.UI;
using UniRx;

public partial class HeroAI : UnitAI {
    public GameObject targetCard;
    GameObject selectedMark;
    bool unitSelected = false;
    bool moving = false;

    private Transform expBar;
    private Transform cooltimeBar;
    private TextMeshPro LvText;
    private decimal attackSP;
    [SerializeField] public ActiveCard unitCard;
    private List<HeroAI> fightHeroes;
    IEnumerator coroutine;
    float bonusStat = 1.0f;

    public override void Init(object card) {
        ConditionSet missionStat = PlayerController.Instance.MissionConditionsController().oppenentConditions.Find(x => x.condition == Conditions.hero_buff);
        if (missionStat != null && gameObject.layer == 11)
            bonusStat = (float)missionStat.args[0] / 100;
        healthBar = transform.Find("UnitBar/HP");
        expBar = transform.Find("UnitBar/Exp");
        cooltimeBar = transform.Find("UnitBar/SkillCool");
        LvText = transform.Find("UnitBar/LevelBackGround/Level").GetComponent<TextMeshPro>();
        unitSpine = GetComponentInChildren<UnitSpine>();
        fightHeroes = new List<HeroAI>();
        InitStatic();
    }

    public override void Init(object card, GameObject cardObj) {
        var clickGroup = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));
        clickGroup.RepeatUntilDestroy(gameObject).Where(_ => !moving && ClickGroup() && !Canvas.FindObjectOfType<ToggleGroup>().AnyTogglesOn()).Subscribe(_ => checkWay());
        Init(card);
        ActiveCard actcard = (ActiveCard)card;
        MaxHealth = actcard.baseSpec.unit.hitPoint;
        HP = MaxHealth;
        this.unitCard = actcard;
        DataModules.Unit unit = actcard.baseSpec.unit;
        int level = (actcard.ev.lv <= 0) ? 1 : actcard.ev.lv;
        SetUnitDataCommon(level);
        SetColliderData();
        SetUnitColor();

        if (cardObj == null) return;
        unitCard.gameObject = cardObj;

        coroutine = UpdateInfoCard();
        StartCoroutine(coroutine);
        setState(aiState.NONE);
        selectedMark = Instantiate(UnityEngine.Resources.Load("Prefabs/MoveArrow") as GameObject, transform);
        selectedMark.SetActive(false);
    }

    /// <summary>
    /// 피아 식별 색상
    /// </summary>
    private void SetUnitColor() {
        var spriteRenderer = transform.Find("UnitBar/Indicator").GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;
        switch (ownerNum) {
            case PlayerController.Player.PLAYER_1:
                spriteRenderer.color = new Color32(0, 105, 253, 255);
                break;
            case PlayerController.Player.PLAYER_2:
                spriteRenderer.color = new Color32(253, 58, 0, 255);
                break;
        }
    }

    //public override void setunitdata(unit unit, int level) {
    //    init(unit);
    //    unitcard = new activecard();
    //    unitcard.basespec.unit = unit;
    //    setunitdatacommon(level);
    //    setcolliderdata();
    //}

    private void SetUnitDataCommon(int level) {
        DataModules.Unit unit = unitCard.baseSpec.unit;
        moveSpeed = unit.moveSpeed;
        attackSpeed = unit.attackSpeed;
        attackRange = unit.attackRange;
        power = Mathf.RoundToInt(unit.attackPower * bonusStat);
        power = PowerUP(power);
        if(unitCard.ev == null) unitCard.ev = new Ev() { lv = level };
        unitCard.ev.lv = level;
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
                Slider healthSlider = unitCard.gameObject.transform.Find("Health").GetComponent<Slider>();
                
                healthSlider.value = HP;
                healthSlider.maxValue = MaxHealth;

                Slider expSlider = unitCard.gameObject.transform.Find("Exp").GetComponent<Slider>();
                expSlider.value = unitCard.ev.exp;
                expSlider.maxValue = ExpNeed();

                unitCard.gameObject.transform.Find("Level/Value").GetComponent<Text>().text = unitCard.ev.lv.ToString();
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
        if(gameObject.layer == 10) {
            ConditionSet expSet = PlayerController.Instance
                .MissionConditionsController()
                .conditions.Find(x => x.condition == Conditions.exp_add);
            if (expSet != null) {
                int percentage = expSet.args[0] / 100;
                exp += exp * percentage;
            }
        }
        
        else if(gameObject.layer == 11) {
            ConditionSet expSet = PlayerController.Instance
                .MissionConditionsController()
                .oppenentConditions.Find(x => x.condition == Conditions.exp_add);
            if (expSet != null) {
                int percentage = expSet.args[0] / 100;
                exp += exp * percentage;
            }
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
        Text cardLvText = unitCard.gameObject.transform.Find("Level/Value").GetComponent<Text>();
        cardLvText.text = unitCard.ev.lv.ToString();
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
        MaxHealth = PowerUP((float)unitCard.baseSpec.unit.hitPoint) * bonusStat;
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
            if (fightHeroes[i] == null || GameObject.ReferenceEquals(fightHeroes[i].gameObject, heroAI.gameObject)) return;
        fightHeroes.Add(heroAI);
    }

    public override HeroAI GetMyHeroAI() {
        return this;
    }

    protected override void GiveExp() {
        if (fightHeroes.Count == 0) return;
        int exp = Mathf.FloorToInt(200f * unitCard.ev.lv * unitCard.baseSpec.unit.rarity / 5f);
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
        DataModules.Unit unit = unitCard.baseSpec.unit;
        moveSpeed = unit.moveSpeed;
        attackSpeed = unit.attackSpeed;
        attackRange = unit.attackRange;
        power = PowerUP(unit.attackPower);
        MaxHealth = PowerUP(unit.hitPoint);
    }

    private bool ClickGroup() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            LayerMask mask = (1 << LayerMask.NameToLayer("Direction")) | (1 << LayerMask.NameToLayer("PlayerUnit"));
            RaycastHit2D hits = Physics2D.Raycast(new Vector2(mousePos.x, mousePos.y), Vector2.zero, Mathf.Infinity, mask);
            if (!hits) {
                //if (unitSelected) checkWay();
                if (unitSelected) {
                    transform.position = new Vector3(mousePos.x, mousePos.y, 0);
                    checkWay();
                }
                return false;
            }
            if (hits.collider.transform != transform && hits.collider.transform.parent != transform && hits.collider.transform.parent.parent != transform) return false;
            if (hits.collider.transform.childCount > 0 && hits.collider.transform.GetChild(2).gameObject.layer != 10) return false;
            if (hits.collider.attachedRigidbody == transform.GetChild(2).GetComponent<Rigidbody2D>())
                return true;

            if (hits.transform.parent.GetComponent<CircleCollider2D>() == transform.parent.GetComponent<CircleCollider2D>())
                return true;

            else if (hits.collider.gameObject.layer == 10) {
                if (!unitSelected) return false;
                int index = hits.collider.transform.GetSiblingIndex();
                
                if (!PlayerController.Instance.FirstMove)
                    PlayerController.Instance.FirstMove = true;
                return true;
            }
            else return false;
        }
        return false;
    }

    public void checkWay() {
        if (PlayerController.Instance.ClickedUnitgroup == null)
            PlayerController.Instance.ClickedUnitgroup = gameObject;
        else if (PlayerController.Instance.ClickedUnitgroup == gameObject)
            PlayerController.Instance.ClickedUnitgroup = null;
        else
            return;
        unitSelected = !unitSelected;
        selectedMark.SetActive(unitSelected);
    }
}
