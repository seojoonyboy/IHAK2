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

    public CreepStation tower;
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
        tower.MonsterDie(gameObject);
        Destroy(gameObject);
    }

    /// <summary>
    /// Idle 상태일때 순찰
    /// </summary>
    /// <returns>목적지</returns>
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