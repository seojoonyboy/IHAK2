using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 중립 몬스터
/// </summary>
public partial class MonsterAI : MonoBehaviour {
    public Transform originPos;
    public Vector2 patrolTarget;

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
    public float health = 0;
    public float maxHealth = 0;
    public Transform healthBar;

    public CreepStation tower;
    MonsterSpine monsterSpine;
    void Awake() {
        originPos = transform;
        monsterSpine = GetComponentInChildren<MonsterSpine>();
    }

    void Start() {
        setState(aiState.NONE);
        patrolTarget = GetPatrolTarget();
        interval += Random.Range(-1.0f, 1.0f);
    }

    void Update() {
        update(Time.deltaTime);
    }

    private void setState(aiState state) {
        update = null;
        switch (state) {
            case aiState.NONE:
                monsterSpine.Idle();
                update = noneUpdate;
                break;
            case aiState.MOVE:
                monsterSpine.Move();
                update = moveUpdate;
                break;
            case aiState.ATTACK:
                monsterSpine.Attack();
                update = attackUpdate;
                break;
        }
    }

    void noneUpdate(float time) {
        if (tower == null) return;
        this.time += time;
        transform.position = Vector2.MoveTowards(
                new Vector2(transform.position.x, transform.position.y),
                patrolTarget,
                speed * Time.deltaTime
        );

        if(Vector2.Distance(transform.position, patrolTarget) == 0.1f) {
            setState(aiState.NONE);
        }

        if (this.time > interval) {
            patrolTarget = GetPatrolTarget();
            this.time = 0;
            interval = 10 + Random.Range(-1.0f, 1.0f);
        }
    }

    void moveUpdate(float time) { }
    void attackUpdate(float time) { }

    public void damaged(float damage) {
        health -= damage;
        calculateHealthBar();
        if (health <= 0) Die();
    }

    public void Healed(float amount) {
        health += amount;
        if (health > maxHealth) health = maxHealth;
        calculateHealthBar();
    }

    public void calculateHealthBar() {
        if (!healthBar.parent.gameObject.activeSelf) healthBar.parent.gameObject.SetActive(true);
        float percent = (float)health / maxHealth;
        healthBar.transform.localScale = new Vector3(percent, 1f, 1f);
    }

    public void Die() {
        tower.MonsterDie(gameObject);
        Destroy(gameObject);
    }

    private Vector2 GetPatrolTarget() {
        if (tower == null) return transform.position;
        int posCount = tower.transform.GetChild(0).childCount;
        int rndNum = Random.Range(0, posCount - 1);

        Transform target = tower.transform.GetChild(0).GetChild(rndNum).transform;
        float offsetX = Random.Range(-10.0f, 10.0f);
        float offsetY = Random.Range(-5.0f, 5.0f);
        Vector2 vector = new Vector2(target.position.x + offsetX, target.position.y + offsetY);
        return vector;
    }
}

public partial class MonsterAI {
    public Unit data;
}