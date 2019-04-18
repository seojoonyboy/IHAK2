using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;

public class FowardHQ : IngameBuilding {

    public CircleCollider2D effectRange;
    private int damage;
    private float atkTime;
    private Transform enemy;

    [SerializeField] bool isAttacking = false;
    [SerializeField] bool isDestroyed = false;

    private float time;
    [SerializeField] private GameObject arrow;

    [SerializeField] [ReadOnly] private PlayerController.Player towerOwner;

    public bool IsDestroyed {
        get { return isDestroyed; }
        set { isDestroyed = true; }
    }

    public PlayerController.Player TowerOwner {
        set { towerOwner = value; }
    }

    // Use this for initialization
    void Start() {
        maxHp = 300;
        buildingHp = maxHp;
        effectRange = transform.parent.GetComponent<CircleCollider2D>();
        damage = 23;
        atkTime = 1.4f;
    }

    private void Update() {
        if (isDestroyed == true) return;
        if(buildingHp <= 0) {
            buildingHp = 0;
            isDestroyed = true;
            return;
        }
        if (isAttacking == false) return;
        if (checkEnemyDead() == true) return;
        time += Time.deltaTime;
        if (time < atkTime) return;
        time -= atkTime;
        shootArrow();
    }

    public void Init(AttackInfo info) {
        effectRange = transform.parent.GetComponent<CircleCollider2D>();
        setRange(info.attackRange);
        damage = info.power;
        atkTime = info.attackSpeed;
    }

    private void setRange(float amount) {
        effectRange.radius = amount;
    }

    private void shootArrow() {
        GameObject arrow = Instantiate(this.arrow, transform.position, Quaternion.identity);
        iTween.MoveTo(arrow, enemy.position, atkTime * 0.3f);
        Destroy(arrow, atkTime * 0.3f);
    }

    private bool checkEnemyDead() {
        if (enemy != null) return false;
        isAttacking = false;
        effectRange.enabled = true;
        return true;
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if(collision.gameObject.layer != (int)towerOwner) {
            if (isAttacking == true) return;
            if (collision.name.CompareTo("Skeleton") == 0) return;
            isAttacking = true;
            enemy = collision.transform;
            time = 0f;
        }
    }

}
