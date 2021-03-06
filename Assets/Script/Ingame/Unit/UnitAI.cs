using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DataModules;
using System;
using PolyNav;

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
    protected Vector3 targetPos;
    [SerializeField] private GameObject arrow;

    public float power = 0;
    protected float defense = 0;
    public float moveSpeed;
    protected float attackSpeed;
    public float attackRange;

    private float currentTime;

    protected static PlayerController playerController;
    protected static IngameDeckShuffler ingameDeckShuffler;
    protected static EnemyHeroGenerator enemyHeroGenerator;
    protected static IngameHpSystem ingameHpSystem;

    protected UnitSpine unitSpine;
    private UnitDetector detector;
    protected IngameSceneEventHandler eventHandler;
    private SpriteMask shaderMask;
    private PolyNavAgent navAgent;


    void Awake() {
        eventHandler = IngameSceneEventHandler.Instance;
        navAgent = GetComponent<PolyNavAgent>();
        if (myLayer > 0) return;
        myLayer = LayerMask.NameToLayer("PlayerUnit");
        enemyLayer = LayerMask.NameToLayer("EnemyUnit");
        neutralLayer = LayerMask.NameToLayer("Neutral");
    }

    void Start() {
        LightSet();
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, ReturnDeck);
    }

    private void LightSet() {
        shaderMask = GetComponentInChildren<SpriteMask>();
        if(gameObject.layer == myLayer) return;
        shaderMask.gameObject.SetActive(false);
    }

    private void OnDestroy() {
        if (standingStation != null) standingStation.DestroyEnteredTarget(gameObject);
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, ReturnDeck);
    }

    protected void InitStatic() {
        if (playerController == null) playerController = FindObjectOfType<PlayerController>();
        if (ingameDeckShuffler == null) ingameDeckShuffler = FindObjectOfType<IngameDeckShuffler>();
        if (enemyHeroGenerator == null) enemyHeroGenerator = FindObjectOfType<EnemyHeroGenerator>();
        if (ingameHpSystem == null) ingameHpSystem = IngameHpSystem.Instance;
    }

    protected void SetColliderData() {
        detector = transform.GetComponentInChildren<UnitDetector>();
        detector.SetData(attackRange, LayertoGive(true));
    }

    protected void setState(aiState state) {
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
                unitSpine.Move();
                update = returnUpdate;
                break;
        }
    }

    private bool StartMove() {
        if(targetUnit == null && targetPos == null) return false;
        Vector3 pos;
        if(targetUnit == null) pos = targetPos;
        else pos = targetUnit.position;
        Vector3 distance = pos - transform.position;
        unitSpine.Move();
        navAgent.SetDestination(targetPos);
        return true;
    }

    void Update() {
        update(Time.deltaTime);
    }

    void noneUpdate(float time) { }

    void moveUpdate(float time) {
        currentTime += time; 
        
        Vector3 pos = targetUnit.position;
        Vector3 distance = pos - transform.position;
        float length = Vector3.Distance(transform.position, pos);
        if (isTargetClose(length)) {
            setState(aiState.ATTACK);
            return;
        }
        unitSpine.SetDirection(distance);
        if(length < 0.5f) setState(aiState.NONE);
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
                if(detector == null) return;
                detector.EnemyDone();
                return;
            }
            attackUnit();
        }
    }

    void returnUpdate(float time) {
        Vector3 distance = Vector3.zero - transform.localPosition;
        float length = Vector3.Distance(transform.localPosition, Vector3.zero);
        if(length < 3f) {
            setState(aiState.NONE);
            return;
        }
        transform.Translate(distance.normalized * time * moveSpeed);
        unitSpine.SetDirection(distance);
    }

    public virtual void attackUnit() {
        AI.SkyNet skyNet = targetUnit.GetComponent<AI.SkyNet>();
        if(skyNet != null) {
            if(skyNet.HP <= 0) {
                targetUnit = null;
                return;
            } 
            skyNet.Damage(power, transform);
        }
        else if(targetUnit.GetComponent<TileObject>()) {
            bool isPlayer1 = myLayer == LayerMask.NameToLayer("PlayerUnit");            
            ingameHpSystem.TakeDamage(isPlayer1 ? IngameHpSystem.Target.ENEMY_1 : IngameHpSystem.Target.ME, Mathf.FloorToInt(power));
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
        Vector3 vector = Vector3.Normalize(targetUnit.position - transform.position);
        if(vector.x > 0) arrow.GetComponent<SpriteRenderer>().flipX = true;
        Destroy(arrow, attackSpeed * 0.3f);
    }

    private bool isTargetClose(float distance) {
        if(targetUnit == null) return false;
        if (distance <= attackRange)
            return true;
        return false;
    }

    public override void Damage(float damage, Transform enemy) {
        base.Damage(damage, enemy);
        if (enemy == null) return;
        CheckisNeedChangeTarget(enemy);
    }

    private void CheckisNeedChangeTarget(Transform enemy) {
        if(targetUnit == null) return;
        IngameBuilding building = targetUnit.GetComponent<IngameBuilding>();
        TileObject HQ = targetUnit.GetComponent<TileObject>();
        bool isBuilding = (building != null || HQ != null);
        if(!isBuilding) return;
        if(enemy.GetComponent<UnitAI>() == null) return;
        targetUnit = enemy;
    }

    public void Heal() {
        float amount = MaxHealth * 0.05f;
        base.Recover(amount);
    }

    public void NearEnemy(Collider2D other) {
        targetUnit = other.transform;
    }

    public virtual HeroAI GetMyHeroAI() {
        return transform.parent.GetComponentInChildren<HeroAI>();
    }

    public void SetDestination(Vector3 pos) {
        targetPos = pos;
        setState(aiState.MOVE);
    }

    public override void Init(object card) { }
    public override void Init(object unit, GameObject gameObject) { }
    public virtual void ReturnDeck(Enum Event_Type, Component Sender, object Param) { }
    public virtual int CalPower() { return Mathf.RoundToInt(power); }
    public virtual void attackingHero(UnitAI unit) { }
}
