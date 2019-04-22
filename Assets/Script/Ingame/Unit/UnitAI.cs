using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DataModules;
using System;

public partial class UnitAI : AI.SkyNet {
    public enum aiState {
        NONE,
        MOVE,
        ATTACK,
        RETURN
    };

    protected delegate void timeUpdate(float time);
    protected timeUpdate update;
    protected Transform targetUnit;
    protected Transform healthBar;
    [SerializeField] private GameObject arrow;

    public float health = 0;
    protected float maxHealth = 0;
    public float power = 0;
    protected float defense = 0;
    public float moveSpeed;
    protected float attackSpeed;
    public float attackRange;

    private float currentTime;

    protected static PlayerController playerController;
    protected static IngameDeckShuffler ingameDeckShuffler;
    protected static EnemyHeroGenerator enemyHeroGenerator;

    protected UnitSpine unitSpine;
    private UnitDetector detector;
    private IngameSceneEventHandler eventHandler;
    protected UnitGroup myGroup;

    protected static int myLayer = 0;
    protected static int enemyLayer = 0;
    protected static int neutralLayer = 0;

    void Awake() {
        eventHandler = IngameSceneEventHandler.Instance;
        if (myLayer > 0) return;
        myLayer = LayerMask.NameToLayer("PlayerUnit");
        enemyLayer = LayerMask.NameToLayer("EnemyUnit");
        neutralLayer = LayerMask.NameToLayer("Neutral");
    }

    void Start() {
        SetEnemy();
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, ReturnDeck);
    }

    private void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, ReturnDeck);
    }

    private void OnEnable() {
        if(myGroup == null) myGroup = GetComponentInParent<UnitGroup>();
        SetEnemy();
    }

    void OnDisable() {
        if(detector == null) return;
        detector.EnemyDone();
    }

    protected void InitStatic() {
        if (playerController == null) playerController = FindObjectOfType<PlayerController>();
        if (ingameDeckShuffler == null) ingameDeckShuffler = FindObjectOfType<IngameDeckShuffler>();
        if (enemyHeroGenerator == null) enemyHeroGenerator = FindObjectOfType<EnemyHeroGenerator>();
    }

    protected void SetColliderData() {
        detector = transform.GetComponentInChildren<UnitDetector>();
        detector.SetData(attackRange, LayertoGive(true));
    }

    public void SearchEnemy() {
        if(myGroup == null) return;
        Transform enemy = myGroup.GiveMeEnemy(transform);
        if(enemy == null) return;
        targetUnit = enemy;
        if(targetUnit == null) Debug.LogWarning("어떤 유령인건지 궁금하니 이종욱에게 말해주세요");
    }

    private void SetEnemy() {
        SearchEnemy();
        if(targetUnit == null) return;
        setState(aiState.MOVE);
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
                if(!StartMove()) break;
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

    private bool StartMove() {
        if(targetUnit == null) return false;
        Transform target = targetUnit.transform;
        Vector3 distance = target.position - transform.position;
        unitSpine.SetDirection(distance);
        unitSpine.Move();
        return true;
    }

    void Update() {
        update(Time.deltaTime);
    }

    void noneUpdate(float time) { }

    void moveUpdate(float time) {
        currentTime += time; 
        if (targetUnit == null) { 
            SetEnemy();
            return;
        }
        Transform target = targetUnit;
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
            distance = Vector3.Distance(targetUnit.position, transform.position);
            if (!isTargetClose(distance)) {
                setState(aiState.MOVE);
                return;
            }
            attackUnit();
        }
        else SetEnemy();
    }

    public virtual void attackUnit() {
        UnitAI unitAI = targetUnit.GetComponent<UnitAI>();
        MonsterAI monsterAI = targetUnit.GetComponent<MonsterAI>();
        IngameBuilding building = targetUnit.GetComponent<IngameBuilding>();
        if(unitAI != null) {
            unitAI.damaged(power, transform); 
            unitAI.attackingHero(this);
            if (unitAI.health <= 0f) {
                targetUnit = null;
                SetEnemy();
            }
        }
        else if(monsterAI != null) {
            monsterAI.Damage(power);
            if (monsterAI.HP <= 0f) {
                targetUnit = null;
                SetEnemy();
            }
        }
        else if(building != null) {
            building.Damage(power);
            if(building.HP <= 0f) {
                targetUnit = null;
                SetEnemy();
            }
        }
        else Debug.LogWarning("유닛도 아닌놈을 타겟으로 잡은건지 이종욱에게 알려주세요 :" + targetUnit.name);
        unitSpine.Attack();
        if(attackRange <= 2f) return;
        shootArrow();
    }

    private void shootArrow() {
        if(targetUnit == null) return;
        GameObject arrow = Instantiate(this.arrow, transform.position, Quaternion.identity);
        iTween.MoveTo(arrow, targetUnit.position, attackSpeed * 0.3f);
        Destroy(arrow, attackSpeed * 0.3f);
    }

    private bool isTargetClose(float distance) {
        if (targetUnit == null) {
            SetEnemy();
            return false;
        }
        if (distance <= attackRange)
            return true;
        return false;
    }

    public virtual void damaged(float damage) {
        health -= damage;
        unitSpine.Hitted();
        calculateHealthBar();
        if (health <= 0) DestoryEnemy();
    }

    public void damaged(float damage, Transform enemy) {
        damaged(damage);
        myGroup.UnitHittedOrFound(enemy);
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
            return isEnemy ?  (1 << enemyLayer) | (1 << neutralLayer) : myLayer;
        else
            return isEnemy ? (1 << myLayer) | (1 << neutralLayer) : enemyLayer;
    }

    protected void calculateHealthBar() {
        if (!healthBar.parent.gameObject.activeSelf) healthBar.parent.gameObject.SetActive(true);
        float percent = (float)health / maxHealth;
        healthBar.transform.localScale = new Vector3(percent, 1f, 1f);
    }

    public void NearEnemy(Collider2D other) {
        //targetUnit = other.GetComponent<UnitAI>();
        myGroup.UnitHittedOrFound(other.transform);
    }

    public override void Init(object card) { }
    public override void SetUnitData(object unit, GameObject gameObject) { }
    public virtual void DestoryEnemy() { }
    public virtual void ReturnDeck(Enum Event_Type, Component Sender, object Param) { }
    public virtual int CalPower() { return Mathf.RoundToInt(power); }
    public virtual void attackingHero(UnitAI unit) { }
}
