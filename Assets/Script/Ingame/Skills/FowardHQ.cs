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
    [SerializeField] private GameObject gauge;
    [SerializeField] private Transform enemy;
    
    [SerializeField] bool isAttacking = false;
    [SerializeField] bool isDestroyed = false;

    [SerializeField] private float time;
    [SerializeField] private GameObject arrow;

    [SerializeField] private PlayerController.Player towerOwner;

    public bool IsDestroyed {
        get { return isDestroyed; }
        set { isDestroyed = true; }
    }

    public PlayerController.Player TowerOwner {
        set { towerOwner = value; }
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
        maxHp = 300;
        buildingHp = maxHp;
        effectRange = transform.parent.GetComponent<CircleCollider2D>();
        damage = 23;
        atkTime = 1.4f;
        towerOwner = PlayerController.Player.NEUTRAL;
        gauge = transform.Find("UnitBar").gameObject;
        setRange(10);
        ObjectActive();
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

    private void shootArrow() {
        if (enemy == null) return;
        GameObject arrow = Instantiate(this.arrow, transform.position, Quaternion.identity);
        iTween.MoveTo(arrow, enemy.position, atkTime * 0.3f);
        Destroy(arrow, atkTime * 0.3f);
        enemy.GetComponent<UnitAI>().damaged(damage, transform);
    }

    private bool checkEnemyDead() {
        if (enemy != null) return false;
        isAttacking = false;
        effectRange.enabled = true;
        return true;
    }

    public override void TakeDamage(float amount) {
        if (isDestroyed == true) return;
        Transform gaugeAmount = gauge.transform.Find("Gauge");

        if (buildingHp > amount)
            buildingHp -= Mathf.FloorToInt(amount);
        else
            buildingHp = 0;

        gaugeAmount.localScale = new Vector3(buildingHp / maxHp, gaugeAmount.localScale.y);
    }

    public void RepairBuilding() {
        if (isDestroyed == false) return;
        buildingHp = maxHp;
        Transform gaugeAmount = gauge.transform.Find("Gauge");
        gaugeAmount.localScale = new Vector3(1, gaugeAmount.localScale.y);
        isDestroyed = false;
    }

    public void ObjectActive() {
        Observable.EveryUpdate().Where(_ => enemy != null).Subscribe(_ => isAttacking = true).AddTo(gameObject);
        Observable.EveryUpdate().Where(_ => enemy == null).Subscribe(_ => { isAttacking = false; time = 0; }).AddTo(gameObject);
        Observable.EveryUpdate().Where(_ => buildingHp >= maxHp).Subscribe(_ => gauge.SetActive(false)).AddTo(gameObject);
        Observable.EveryUpdate().Where(_ => buildingHp < maxHp).Subscribe(_ => gauge.SetActive(true)).AddTo(gameObject);
        Observable.EveryUpdate().Where(_ => buildingHp <= 0).Subscribe(_ => isDestroyed = true).AddTo(gameObject);       
        
    }
    
}
