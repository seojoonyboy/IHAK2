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
    private IngameSceneEventHandler eventHandler;
    public UnitGroup myGroup;


    void Awake() {
        eventHandler = IngameSceneEventHandler.Instance;
        if (myLayer > 0) return;
        myLayer = LayerMask.NameToLayer("PlayerUnit");
        enemyLayer = LayerMask.NameToLayer("EnemyUnit");
        neutralLayer = LayerMask.NameToLayer("Neutral");
    }

    void Start() {
        if(myGroup == null) myGroup = GetComponentInParent<UnitGroup>();
        eventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, ReturnDeck);
    }

    private void OnDestroy() {
        if (standingStation != null) standingStation.DestroyEnteredTarget(gameObject);
        eventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.ORDER_UNIT_RETURN, ReturnDeck);
    }

    public void Battle(bool isBattle) {
        if(isBattle) {
            SetEnemy();
        }
        else  {
            setState(aiState.RETURN);
            if(detector == null) return;
            detector.EnemyDone();
        }
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
                unitSpine.Move();
                update = returnUpdate;
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

    public override void Damage(float damage, Transform enemy) {
        base.Damage(damage, enemy);
        myGroup.UnitHittedOrFound(enemy);
    }

    public void Heal() {
        float amount = MaxHealth * 0.05f;
        base.Recover(amount);
    }

    public void NearEnemy(Collider2D other) {
        //targetUnit = other.GetComponent<UnitAI>();
        myGroup.UnitHittedOrFound(other.transform);
    }

    public virtual HeroAI GetMyHeroAI() {
        return transform.parent.GetComponentInChildren<HeroAI>();
    }

    public override void Init(object card) { }
    public override void Init(object unit, GameObject gameObject) { }
    public virtual void ReturnDeck(Enum Event_Type, Component Sender, object Param) { }
    public virtual int CalPower() { return Mathf.RoundToInt(power); }
    public virtual void attackingHero(UnitAI unit) { }
}
