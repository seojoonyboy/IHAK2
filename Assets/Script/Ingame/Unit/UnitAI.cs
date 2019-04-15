using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DataModules;
using System;

public partial class UnitAI : MonoBehaviour {
    public enum aiState {
        NONE,
        MOVE,
        ATTACK,
        RETURN
    };

    protected delegate void timeUpdate(float time);
    protected timeUpdate update;
    protected UnitAI targetUnit;
    protected Transform healthBar;

    public float health = 0;
    protected float maxHealth = 0;
    public float power = 0;
    protected float defense = 0;
    public float moveSpeed;
    protected float attackSpeed;
    protected float attackRange;

    private float currentTime;

    protected static PlayerController playerController;
    protected static IngameDeckShuffler ingameDeckShuffler;
    protected static EnemyHeroGenerator enemyHeroGenerator;

    protected UnitSpine unitSpine;
    private IngameSceneEventHandler eventHandler;
    private UnitGroup myGroup;

    protected static int myLayer = 0;
    protected static int enemyLayer = 0;

    void Awake() {
        eventHandler = IngameSceneEventHandler.Instance;
        if (myLayer > 0) return;
        myLayer = LayerMask.NameToLayer("PlayerUnit");
        enemyLayer = LayerMask.NameToLayer("EnemyUnit");
    }

    void Start() {
        setState(aiState.NONE);
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, ReturnDeck);
    }

    private void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, ReturnDeck);
    }

    private void OnEnable() {
        if(myGroup == null) myGroup = GetComponentInParent<UnitGroup>();
        SearchEnemy();
        setState(aiState.MOVE);
    }

    protected void InitStatic() {
        if (playerController == null) playerController = FindObjectOfType<PlayerController>();
        if (ingameDeckShuffler == null) ingameDeckShuffler = FindObjectOfType<IngameDeckShuffler>();
        if (enemyHeroGenerator == null) enemyHeroGenerator = FindObjectOfType<EnemyHeroGenerator>();
    }

    public void SearchEnemy() {
        Transform enemy = myGroup.GiveMeEnemy(transform);
        targetUnit = enemy.GetComponent<UnitAI>();
        if(targetUnit == null) Debug.LogWarning("유닛 말고 또 뭔지 이종욱에게 말해주세요"); //TODO : 유닛 말고 또 다른 종류의 적
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
                Transform target = targetUnit.transform;
                Vector3 distance = target.position - transform.position;
                unitSpine.SetDirection(distance);
                unitSpine.Move();
                update = moveUpdate;
                break;
            case aiState.ATTACK:
                unitSpine.Idle();
                update = attackUpdate;
                break;
            case aiState.RETURN:
                //TODO : 유닛이 정리 됐을 때 원위치로 돌아오게 하기
                update = noneUpdate;
                break;
        }
    }

    void Update() {
        update(Time.deltaTime);
    }

    void noneUpdate(float time) { }

    void moveUpdate(float time) {
        currentTime += time; 
        if (targetUnit == null) { 
            return;
        }
        Transform target = targetUnit.transform;
        Vector3 distance = target.position - transform.position;
        float length = Vector3.Distance(transform.position, target.position);
        if (isTargetClose(length)) {
            setState(aiState.ATTACK);
            return;
        }
        transform.Translate(distance.normalized * time * moveSpeed);
        if (currentTime < 2f) return;
        currentTime = 0f;
    }

    void attackUpdate(float time) {
        currentTime += time;
        if (currentTime < attackSpeed) return;
        currentTime = 0f;
        float distance;
        if (targetUnit != null) {
            distance = Vector3.Distance(targetUnit.transform.position, transform.position);
            if (!isTargetClose(distance)) {
                setState(aiState.MOVE);
                return;
            }
            attackUnit();
        }
    }

    public virtual void attackUnit() {
        targetUnit.damaged(power);
        targetUnit.attackingHero(this);
        unitSpine.Attack();
        if (targetUnit.health <= 0f) {
            targetUnit = null;
            setState(aiState.NONE);
        }
    }

    private bool isTargetClose(float distance) {
        if (targetUnit == null) {
            return false;
        }
        if (distance <= attackRange)
            return true;
        return false;
    }

    public virtual void damaged(float damage) {
        health -= damage;
        //unitSpine.Hitted();
        //calculateHealthBar();
        if (health <= 0) DestoryEnemy();
    }

    public void Healed() {
        float amount = maxHealth * 0.05f;
        health += amount;
        if (health > maxHealth) health = maxHealth;
    }

    protected void Healed(float healingHP) {
        health += healingHP;
        if (health > maxHealth) health = maxHealth;
        calculateHealthBar();
    }

    protected int LayertoGive(bool isEnemy) {
        if(gameObject.layer == myLayer)
            return isEnemy ? enemyLayer : myLayer;
        else
            return isEnemy ? myLayer : enemyLayer;
    }

    protected void calculateHealthBar() {
        if (!healthBar.parent.gameObject.activeSelf) healthBar.parent.gameObject.SetActive(true);
        float percent = (float)health / maxHealth;
        healthBar.transform.localScale = new Vector3(percent, 1f, 1f);
    }

    public virtual void SetUnitData(ActiveCard card, GameObject cardObj) { }
    public virtual void SetUnitData(Unit unit, int level) { }
    public virtual void DestoryEnemy() { }
    public virtual void ReturnDeck(Enum Event_Type, Component Sender, object Param) { }
    public virtual int CalPower() { return Mathf.RoundToInt(power); }
    public virtual void attackingHero(UnitAI unit) { }
}
