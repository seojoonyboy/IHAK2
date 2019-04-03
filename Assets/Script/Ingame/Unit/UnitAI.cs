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
        DEAD
    };

    protected delegate void timeUpdate(float time);
    protected timeUpdate update;

    private BuildingInfo targetBuilding;
    protected UnitAI targetUnit;
    protected Transform healthBar;
    
    public float health = 0;
    protected float maxHealth = 0;
    public float power = 0;
    protected float defense = 0;
    protected float moveSpeed;
    protected float attackSpeed;
    protected float attackRange;

    private float currentTime;

    protected static EnemyBuildings enemyBuildings;
    protected static PlayerController playerController;
    protected static IngameDeckShuffler ingameDeckShuffler;
    protected static EnemyHeroGenerator enemyHeroGenerator;
    protected static IngameHpSystem ingameHpSystem;

    public GameObject ontile;
    private CircleCollider2D detectCollider;

    private List<BuildingInfo> buildingInfos;
    
    protected UnitSpine unitSpine;
    private IngameSceneEventHandler eventHandler;

    protected static int myLayer = 0;
    protected static int enemyLayer = 0;
    private IngameHpSystem.Target targetEnum;

    void Awake() {
        eventHandler = IngameSceneEventHandler.Instance;
        if(myLayer > 0) return;
        myLayer = LayerMask.NameToLayer("PlayerUnit");
        enemyLayer = LayerMask.NameToLayer("EnemyUnit");
    }

    void Start() {
        detectCollider = transform.GetComponentInChildren<CircleCollider2D>();
        detectCollider.radius = attackRange * 1.5f;
        if (gameObject.layer == myLayer) setUnitPlayer(enemyBuildings.buildingInfos, enemyLayer, myLayer, IngameHpSystem.Target.ENEMY_1);
        else if (gameObject.layer == enemyLayer) setUnitPlayer(playerController.playerBuildings().buildingInfos, myLayer, enemyLayer, IngameHpSystem.Target.ME);
        if (searchTarget()) setState(aiState.MOVE);
        else setState(aiState.NONE);
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, ReturnDeck);
    }

    private void setUnitPlayer(List<BuildingInfo> info, int layer1, int layer2, IngameHpSystem.Target targetEnum) {
        buildingInfos = info;
        UnitDetector dectector = GetComponentInChildren<UnitDetector>();
        dectector.detectingLayer = layer1;
        dectector.gameObject.layer = layer2;
        this.targetEnum = targetEnum;
    }

    private void OnDestroy() {
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, ReturnDeck);
    }

    protected void InitStatic() {
        if (enemyBuildings == null) enemyBuildings = FindObjectOfType<EnemyBuildings>();
        if (playerController == null) playerController = FindObjectOfType<PlayerController>();
        if (ingameDeckShuffler == null) ingameDeckShuffler = FindObjectOfType<IngameDeckShuffler>();
        if (enemyHeroGenerator == null) enemyHeroGenerator = FindObjectOfType<EnemyHeroGenerator>();
        if (ingameHpSystem == null) ingameHpSystem = FindObjectOfType<IngameHpSystem>();
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
                Vector3 distance = target.position - transform.position;
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
        Debug.Log("updating UnitAI");
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
        Vector3 distance = target.position - transform.position;
        float length = Vector3.Distance(transform.position, target.position);
        if (isTargetClose(length)) {
            setState(aiState.ATTACK);
            return;
        }
        transform.Translate(distance.normalized * time * moveSpeed);
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
        if (currentTime < attackSpeed) return;
        currentTime = 0f;
        float distance;
        if (targetUnit != null) {
            distance = Vector3.Distance(targetUnit.transform.position, transform.position);
            if(!isTargetClose(distance)) {
                setState(aiState.MOVE);
                return;
            }
            attackUnit();
        }
        else if (targetBuilding != null) {
            distance = Vector3.Distance(targetBuilding.gameObject.transform.position, transform.position);
            if(!isTargetClose(distance)) {
                setState(aiState.MOVE);
                return;
            }
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
        ingameHpSystem.TakeDamage(targetEnum, 12, CalPower());
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
        targetUnit.damaged(power);
        targetUnit.attackingHero(this);
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
        if (distance <= attackRange)
            return true;
        return false;
    }

    private bool searchTarget() {
        searchBuilding();
        return targetBuilding != null;
    }
    public void NearEnemy(Collider2D other) {
        targetUnit = other.GetComponent<UnitAI>();
        targetBuilding = null;
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

    public virtual void damaged(float damage) {
        health -= damage;
        unitSpine.Hitted();
        calculateHealthBar();
        if(health <= 0) DestoryEnemy();
    }

    protected void healed(float healingHP) {
        health += healingHP;
        if(health > maxHealth) health = maxHealth;
        calculateHealthBar();
    }

    protected int LayertoGive(bool isEnemy) {
        return isEnemy ? enemyLayer : myLayer;
    }

    protected void calculateHealthBar() {
        if (!healthBar.parent.gameObject.activeSelf) healthBar.parent.gameObject.SetActive(true);
        float percent = (float)health / maxHealth;
        healthBar.transform.localScale = new Vector3(percent, 1f, 1f);
    }

    protected void TileReset() {
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
    }

    public virtual void SetUnitData(ActiveCard card) {}
    public virtual void SetUnitData(Unit unit, int level) {}
    public virtual void DestoryEnemy() {}
    public virtual void ReturnDeck(Enum Event_Type, Component Sender, object Param) {}
    public virtual int CalPower() { return Mathf.RoundToInt(power); }
    public virtual void attackingHero(UnitAI unit) {}
}
