using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DataModules;
using System;
using TMPro;

public class UnitAI : MonoBehaviour {
    public enum aiState {
        NONE,
        MOVE,
        ATTACK,
        DEAD
    };

    private delegate void timeUpdate(float time);
    private timeUpdate update;
    private Transform healthBar;
    private Transform expBar;
    private TextMeshPro LvText;
    private BuildingInfo targetBuilding;
    private UnitAI targetUnit;
    private float maxHealth = 0;
    public float health = 0;
    private float power = 0;
    private float defense = 0;
    private float moveSpeed;
    private float currentTime;
    [SerializeField] private ActiveCard unitCard;

    private static IngameCityManager cityManager;
    private static PlayerController playerController;
    private static Magnification unitMagnificate;
    private static IngameDeckShuffler ingameDeckShuffler;
    private static EnemyHeroGenerator enemyHeroGenerator;

    public GameObject ontile;
    public bool protecting = false;
    private CircleCollider2D detectCollider;

    private List<BuildingInfo> buildingInfos;
    //private IngameCityManager.Target targetEnum;
    private UnitSpine unitSpine;
    private IngameSceneEventHandler eventHandler;

    void Awake() {
        eventHandler = IngameSceneEventHandler.Instance;
    }

    void Start() {
        detectCollider = transform.GetComponentInChildren<CircleCollider2D>();
        detectCollider.radius = unitCard.baseSpec.unit.attackRange;
        if (protecting) detectCollider.enabled = false;
        if (gameObject.layer == LayerMask.NameToLayer("PlayerUnit")) {
            buildingInfos = cityManager.enemyBuildingsInfo;
            SpriteRenderer unitgaugeColor = transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>();
            //targetEnum = IngameCityManager.Target.ENEMY_1;
            GetComponentInChildren<UnitDetector>().detectingLayer = LayerMask.NameToLayer("EnemyUnit");
            GetComponentInChildren<UnitDetector>().gameObject.layer = LayerMask.NameToLayer("PlayerUnit");

        }
        if (gameObject.layer == LayerMask.NameToLayer("EnemyUnit")) {
            buildingInfos = playerController.playerBuildings().buildingInfos;
            SpriteRenderer unitgaugeColor = transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>();
            //targetEnum = IngameCityManager.Target.ME;
            GetComponentInChildren<UnitDetector>().detectingLayer = LayerMask.NameToLayer("PlayerUnit");
            GetComponentInChildren<UnitDetector>().gameObject.layer = LayerMask.NameToLayer("EnemyUnit");
        }
        if (searchTarget())
            setState(aiState.MOVE);
        else
            setState(aiState.NONE);

        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, ReturnDeck);
    }

    private void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, ReturnDeck);
    }

    private void Init() {
        if (healthBar != null) return;
        healthBar = transform.Find("UnitBar/HP");
        expBar = transform.Find("UnitBar/Exp");
        LvText = transform.Find("UnitBar/LevelBackGround/Level").GetComponent<TextMeshPro>();
        unitSpine = GetComponentInChildren<UnitSpine>();
        if (cityManager == null) cityManager = FindObjectOfType<IngameCityManager>();
        if (playerController == null) playerController = FindObjectOfType<PlayerController>();
        if (ingameDeckShuffler == null) ingameDeckShuffler = FindObjectOfType<IngameDeckShuffler>();
        if (enemyHeroGenerator == null) enemyHeroGenerator = FindObjectOfType<EnemyHeroGenerator>();
        if (unitMagnificate == null) unitMagnificate = cityManager.SearchMags("Military");
    }

    public void SetUnitData(ActiveCard card) {
        Init();
        this.unitCard = card;
        Unit unit = card.baseSpec.unit;
        moveSpeed = unit.moveSpeed;
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
    }

    public void SetUnitData(Unit unit, int level) {
        Init();
        unitCard = new ActiveCard();
        unitCard.baseSpec.unit = unit;
        moveSpeed = unit.moveSpeed;
        power = unit.power;
        unitCard.ev = new Ev() { lv = level };
        SetMaxHP();
        if(health == 0) health = unitCard.ev.hp;
        if(health == 0) health = maxHealth;
        else health += HealTime();
        if(health > maxHealth) health = maxHealth;
        calculateHealthBar();
        calculateExpBar();
        ChangeLvText();
    }

    private void LvUpHP() { //레벨업 했을 때 최대체력 변화와 그에 따른 체력 추가를 보는것.
        float beforeMax = maxHealth;
        SetMaxHP();
        beforeMax = maxHealth - beforeMax;
        health += beforeMax;
    }

    private void SetMaxHP() {
        maxHealth = PowerUP((float)unitCard.baseSpec.unit.hitPoint);
    }

    private void setState(aiState state) {
        update = null;
        currentTime = 0f;
        switch (state) {
            case aiState.NONE:
                unitSpine.Idle();
                update = noneUpdate;
                break;
            case aiState.MOVE:
                Transform target = GetTarget();
                Vector3 distance = target.localPosition - transform.localPosition;
                unitSpine.SetDirection(distance);
                unitSpine.Move();
                update = moveUpdate;
                break;
            case aiState.ATTACK:
                unitSpine.Idle();
                update = attackUpdate;
                break;
            case aiState.DEAD:
                update = noneUpdate;
                break;
        }
    }

    void Update() {
        update(Time.deltaTime);
    }

    void noneUpdate(float time) {
        currentTime += time;
        if ((int)currentTime % 3 == 2)
            if (searchTarget()) setState(aiState.MOVE);
        return;
    }

    void moveUpdate(float time) {
        currentTime += time;
        Transform target = GetTarget();
        if (target == null) return;
        Vector3 distance = target.localPosition - transform.localPosition;
        float length = Vector3.Distance(transform.localPosition, target.localPosition);
        if (isTargetClose(length)) {
            setState(aiState.ATTACK);
            return;
        }
        transform.Translate(distance.normalized * time * unitCard.baseSpec.unit.moveSpeed);
        if (currentTime < 2f) return;
        currentTime = 0f;
        searchTarget();
    }

    private Transform GetTarget() {
        Transform target;
        if (targetUnit == null) {
            if (targetBuilding == null) {
                if (!searchTarget()) setState(aiState.NONE);
                return null;
            }
            target = targetBuilding.gameObject.transform.parent;
        }
        else {
            if (targetUnit == null) {
                if (!searchTarget()) setState(aiState.NONE);
                return null;
            }
            target = targetUnit.gameObject.transform;
        }
        return target;
    }

    void attackUpdate(float time) {
        currentTime += time;
        if (currentTime < unitCard.baseSpec.unit.attackSpeed) return;
        currentTime = 0f;
        ExpGain(CalPower());
        if (targetUnit != null) {
            attackUnit();
        }
        else if (targetBuilding != null) {
            attackBuilding();
        }
        else if (targetUnit == null && targetBuilding == null) {
            detectCollider.enabled = true;
            if (searchTarget())
                setState(aiState.MOVE);
            else
                setState(aiState.NONE);
            return;
        }
    }

    private void attackBuilding() {
        //cityManager.TakeDamage(targetEnum, targetBuilding.tileNum, CalPower());
        unitSpine.Attack();
        if (targetBuilding.hp <= 0) {
            targetBuilding = null;
            if (searchTarget())
                setState(aiState.MOVE);
            else
                setState(aiState.NONE);
        }
    }

    private void attackUnit() {
        targetUnit.damaged(CalPower());
        unitSpine.Attack();
        if (targetUnit.health <= 0f) {
            targetUnit = null;
            detectCollider.enabled = true;
            if (searchTarget())
                setState(aiState.MOVE);
            else
                setState(aiState.NONE);
        }
    }

    private bool isTargetClose(float distance) {
        if (targetBuilding == null && targetUnit == null) {
            searchTarget();
            return false;
        }
        if (distance <= unitCard.baseSpec.unit.attackRange)
            return true;
        return false;
    }

    private bool searchTarget() {
        if (protecting) {
            searchUnit();
            return targetUnit != null;
        }
        else {
            searchBuilding();
            return targetBuilding != null;
        }
    }

    private void searchUnit() {
        float distance = 0f;
        UnitAI[] units = transform.parent.GetComponentsInChildren<UnitAI>();
        foreach (UnitAI unit in units) {
            if (unit.gameObject.layer == LayerMask.NameToLayer("PlayerUnit")) continue;
            Vector3 UnitPos = unit.gameObject.transform.position;
            float length = Vector3.Distance(transform.position, UnitPos);
            if (targetUnit == null) {
                targetUnit = unit;
                distance = length;
                continue;
            }
            if (distance > length) {
                targetUnit = unit;
                distance = length;
                continue;
            }
        }
    }

    private void searchBuilding() {
        float distance = 0f;
        foreach (BuildingInfo target in buildingInfos) {
            if (target.hp <= 0) continue;

            Vector3 buildingPos = target.gameObject.transform.parent.position;
            float length = Vector3.Distance(transform.position, buildingPos);
            if (this.targetBuilding == null) {
                this.targetBuilding = target;
                distance = length;
                continue;
            }
            if (distance > length) {
                this.targetBuilding = target;
                distance = length;
                continue;
            }
        }
    }

    public void damaged(int damage) {
        health -= damage;
        ExpGain(damage);
        unitSpine.Hitted();
        calculateHealthBar();
        if(health <= 0) DestoryEnemy();
    }

    private void calculateHealthBar() {
        if (!healthBar.parent.gameObject.activeSelf) healthBar.parent.gameObject.SetActive(true);
        float percent = (float)health / maxHealth;
        healthBar.transform.localScale = new Vector3(percent, 1f, 1f);
    }

    private void calculateExpBar() {
        float percent = (float)unitCard.ev.exp / ExpNeed();
        expBar.transform.localScale = new Vector3(percent, 1f, 1f);
    }

    public void DestoryEnemy() {
        if (ontile == null) {
            ontile = null;
        }
        else if (ontile.GetComponent<TileCollision>() != null && ontile.GetComponent<TileCollision>().count > 0) {
            ontile.GetComponent<TileCollision>().count--;

            if (ontile.GetComponent<TileCollision>().count <= 0) {
                ontile.GetComponent<TileCollision>().count = 0;
                ontile.GetComponent<TileCollision>().check = false;
            }
        }
        unitCard.ev.hp = 0;

        if(gameObject.layer == 10) {
            ingameDeckShuffler.HeroReturn(unitCard.parentBuilding, true);
        }
        else if(gameObject.layer == 11) {
            enemyHeroGenerator.HeroReturn(unitCard.baseSpec.unit.id);
        }
        Destroy(gameObject);
    }

    public void ReturnDeck(Enum Event_Type, Component Sender, object Param) {
        unitCard.ev.hp = (int)health;
        unitCard.ev.time = (int)Time.realtimeSinceStartup;
        ingameDeckShuffler.HeroReturn(unitCard.parentBuilding, false);
        Destroy(gameObject);
    }

    public void NearEnemy(Collider2D other) {
        targetUnit = other.GetComponent<UnitAI>();
        targetBuilding = null;
    }

    private void ExpGain(int exp) {
        exp = Mathf.RoundToInt(exp * 0.2f);
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

    private int CalPower() {
        return Mathf.RoundToInt(power * (gameObject.layer == LayerMask.NameToLayer("PlayerUnit") ? unitMagnificate.magnfication : 1f));
    }

    private float HealTime() {
        int totalTime = (int)Time.realtimeSinceStartup - unitCard.ev.time;
        float healed = maxHealth * totalTime * 0.03f; 
        return healed;
    }

}
