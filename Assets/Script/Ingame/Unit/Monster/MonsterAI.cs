using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 중립 몬스터
/// </summary>
public partial class MonsterAI : MonoBehaviour {
    public Transform originPos;

    public enum aiState {
        NONE,
        MOVE,
        ATTACK,
        RETURN
    };

    protected delegate void timeUpdate(float time);
    protected timeUpdate update;

    private CircleCollider2D detectCollider;
    public float health = 0;
    public float maxHealth = 0;
    public Transform healthBar;

    public MonsterTower tower;
    void Start() {
        setState(aiState.NONE);
    }

    void Update() {
        update(Time.deltaTime);
    }

    private void setState(aiState state) {
        update = null;
        switch (state) {
            case aiState.NONE:
                update = noneUpdate;
                break;
            case aiState.MOVE:
                update = moveUpdate;
                break;
            case aiState.ATTACK:
                update = attackUpdate;
                break;
            case aiState.RETURN:
                update = noneUpdate;
                break;
        }
    }

    void noneUpdate(float time) { }
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
}

public partial class MonsterAI {
    public Unit data;
}