using AI;
using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 중립 몬스터
/// </summary>
public partial class MonsterAI : SkyNet {
    public Transform originPos;
    public Vector2 patrolTarget;
    public Transform moveToTarget;

    private float time;
    private float interval = 10.0f;

    private float speed = 10;
    public enum aiState {
        NONE,
        MOVE,
        ATTACK
    };

    protected delegate void timeUpdate(float time);
    protected timeUpdate update;

    private CircleCollider2D detectCollider;

    public Object tower;
    public MonsterSpine monsterSpine;

    public override void Die() {
        base.Die();

        if(tower.GetType() == typeof(CreepStation)) {
            ((CreepStation)tower).MonsterDie(gameObject);
        }
        if (tower.GetType() == typeof(BaseCampStation)) {
            ((BaseCampStation)tower).MonsterDie(gameObject);
        }
        Destroy(gameObject);
    }
}

public partial class MonsterAI : SkyNet {
    public NeutralMonsterData data;

    public override void Init(object data) {
        NeutralMonsterData NeutralData = (NeutralMonsterData)data;
        if (NeutralData == null) return;

        healthBar = transform.Find("UnitBar/HP").transform;
        monsterSpine = GetComponentInChildren<MonsterSpine>();

        this.data = NeutralData;
        HP = MaxHealth = this.data.hitPoint;
        CalculateHealthBar();
        speed = this.data.moveSpeed;

        GetComponent<CircleCollider2D>().radius = this.data.attackRange;
    }
}