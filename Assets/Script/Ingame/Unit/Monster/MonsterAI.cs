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
    public float health = 0;
    public float maxHealth = 0;
    public Transform healthBar;

    public Object tower;
    public MonsterSpine monsterSpine;

    void Awake() {
        monsterSpine = GetComponentInChildren<MonsterSpine>();
    }

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
        if(tower.GetType() == typeof(CreepStation)) {
            ((CreepStation)tower).MonsterDie(gameObject);
        }
        if (tower.GetType() == typeof(BaseCampStation)) {
            ((BaseCampStation)tower).MonsterDie(gameObject);
        }
        Destroy(gameObject);
    }
}

public partial class MonsterAI {
    //public Unit data;
    public NeutralMonsterData data;

    public void Init(NeutralMonsterData data) {
        if (data == null) return;

        this.data = data;
        health = maxHealth = this.data.hitPoint;
        calculateHealthBar();

        GetComponent<CircleCollider2D>().radius = this.data.attackRange;
    }
}