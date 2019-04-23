using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using UniRx;

public class FowardHQ : IngameBuilding {

    public CircleCollider2D effectRange;
    private int damage;
    private float atkTime;

    [SerializeField] private Transform enemy;
    
    [SerializeField] bool isAttacking = false;
    [SerializeField] bool isDestroyed = false;

    [SerializeField] private float time;
    [SerializeField] private GameObject arrow;

    public bool IsDestroyed {
        get { return isDestroyed; }
        set { isDestroyed = true; }
    }

    public Transform Enemy {
        get {
            if (enemy != null)
                return enemy;

            return null;
        }
        set { enemy = value; }
    }

    // Use this for initialization
    void Start() {
        healthBar = transform.Find("UnitBar/Gauge");
        healthBar.gameObject.SetActive(true);

        HP = MaxHealth = 300;
        effectRange = transform.parent.GetComponent<CircleCollider2D>();
        damage = 23;
        atkTime = 1.4f;
        ownerNum = PlayerController.Player.NEUTRAL;
        setRange(10);
        ObjectActive();
        healthBar.gameObject.SetActive(false);
    }

    private void Update() {
        if (isDestroyed == true) return;
        if(HP <= 0) {
            HP = 0;
            isDestroyed = true;
            return;
        }
        if (isAttacking == false) return;
        if (checkEnemyDead() == true) return;
        time += Time.deltaTime;
        if (time < atkTime) return;
        time = 0;
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

    public void Heal() {
        float amount = MaxHealth * 0.05f;
        base.Recover(amount);
    }

    private void shootArrow() {
        if (enemy == null) return;
        GameObject arrow = Instantiate(this.arrow, transform.position, Quaternion.identity);
        iTween.MoveTo(arrow, enemy.position, atkTime * 0.3f);
        Destroy(arrow, atkTime * 0.3f);
        enemy.GetComponent<UnitAI>().Damage(damage, transform);
    }

    private bool checkEnemyDead() {
        if (enemy != null) return false;
        isAttacking = false;
        effectRange.enabled = true;
        return true;
    }

    public void RepairBuilding(float amount) {
        base.Recover(amount);
        isDestroyed = false;
    }

    public new void ObjectActive() {
        base.ObjectActive();

        Observable.EveryUpdate().Where(_ => enemy != null).Subscribe(_ => isAttacking = true).AddTo(gameObject);
        Observable.EveryUpdate().Where(_ => enemy == null).Subscribe(_ => { isAttacking = false; time = 0; }).AddTo(gameObject);
        Observable.EveryUpdate().Where(_ => HP <= 0).Subscribe(_ => isDestroyed = true).AddTo(gameObject);
    }
    
    
}
