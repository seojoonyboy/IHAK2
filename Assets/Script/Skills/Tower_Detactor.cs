using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_Detactor : MonoBehaviour {
    private CircleCollider2D box;
    private int damage;
    private float atkTime;
    private Transform enemy;
    private bool isAttacking = false;
    private float time;
    [SerializeField]
    private GameObject arrow;

    void Start() {
        box = GetComponent<CircleCollider2D>();
        setRange(40);
        damage = 8;
        atkTime = 0.6f;
    }

    public void init(AttackInfo info) {
        box = GetComponent<CircleCollider2D>();
        setRange(info.attackRange);
        damage = info.power;
        atkTime = info.attackSpeed;
    }

    private void setRange(float range) {
        box.radius = range;
    }
    private void OnTriggerStay2D(Collider2D other) {
        if (isAttacking) return;
        isAttacking = true;
        //enemy = other.transform.parent;
        enemy = other.transform;
        Debug.Log(enemy.name);
        time = 0f;
        box.enabled = false;
    }

    private void Update() {
        if (!isAttacking) return;
        if (checkEnemyDead()) return;
        time += Time.deltaTime;
        if (time < atkTime) return;
        time -= atkTime;
        enemy.SendMessage("damaged", damage, SendMessageOptions.DontRequireReceiver);
        shootArrow();
    }

    private void shootArrow() {
        GameObject arrow = Instantiate(this.arrow, transform.position, Quaternion.identity);
        iTween.MoveTo(arrow, enemy.position, atkTime * 0.5f);
        Destroy(arrow, atkTime * 0.5f);
    }

    private bool checkEnemyDead() {
        if (enemy != null) return false;
        isAttacking = false;
        box.enabled = true;
        return true;
    }
}
