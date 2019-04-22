using Sirenix.OdinInspector;
using DataModules;
using TMPro;
using UnityEngine;
using UniRx;

public class Tower_Detactor : IngameBuilding {
    private CircleCollider2D box;
    private int damage;
    private float atkTime;
    [SerializeField] [ReadOnly] private Transform enemy;

    private bool isAttacking = false;
    [SerializeField] [ReadOnly] private float time;
    [SerializeField]
    private GameObject arrow;

    [SerializeField] [ReadOnly] private PlayerController.Player towerOwner;
    [SerializeField] [ReadOnly] private bool isDestroyed = false;

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

    public void Init(AttackInfo info) {
        base.Init(info);

        HP = MaxHealth = 300;
        box = transform.parent.GetComponent<CircleCollider2D>();
        setRange(10);
        damage = 23;
        atkTime = 1.4f;
        towerOwner = PlayerController.Player.NEUTRAL;
        healthBar = transform.Find("UnitBar");
        ObjectActive();
    }

    private void setRange(float range) {
        box.radius = range;
    }
    /*
    private void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.layer != (int)towerOwner) {
            if (isAttacking) return;
            if (other.name.CompareTo("Skeleton") == 0) return;
            isAttacking = true;
            //enemy = other.transform.parent;
            enemy = other.transform;
            Debug.Log(enemy.name);
            time = 0f;
            box.enabled = false;
        }
    }
    */
    private void Update() {
        if (isDestroyed) return;
        if (HP == 0) {
            isDestroyed = true;
            return;
        }
        if (!isAttacking) return;
        if (checkEnemyDead()) return;
        time += Time.deltaTime;
        if (time < atkTime) return;
        //if (towerShellCount <= 0) return;
        time -= atkTime;
        //enemy.GetComponent<UnitAI>().damaged(damage);
        shootArrow();
    }



    private void shootArrow() {
        if (enemy == null) return;
        GameObject arrow = Instantiate(this.arrow, transform.position, Quaternion.identity);
        iTween.MoveTo(arrow, enemy.position, atkTime * 0.3f);
        Destroy(arrow, atkTime * 0.3f);
        enemy.GetComponent<UnitAI>().damaged(damage, transform);
        //towerShellCount--;
        /*
        TextMeshPro ammoValueText = transform.parent.GetChild(2).GetComponent<TextMeshPro>();
        if(towerShellCount < towerMaxShell) {
            ammoValueText.transform.gameObject.SetActive(true);
            ammoValueText.text = towerShellCount + " / " + towerMaxShell;
        }
        */
    }

    private bool checkEnemyDead() {
        if (enemy != null) return false;
        isAttacking = false;
        box.enabled = true;
        return true;
    }

    public void ObjectActive() {
        Observable.EveryUpdate().Where(_ => enemy != null).Subscribe(_ => isAttacking = true).AddTo(gameObject);
        Observable.EveryUpdate().Where(_ => enemy == null).Subscribe(_ => { isAttacking = false; time = 0; }).AddTo(gameObject);
        Observable.EveryUpdate().Where(_ => HP >= MaxHealth).Subscribe(_ => healthBar.gameObject.SetActive(false)).AddTo(gameObject);
        Observable.EveryUpdate().Where(_ => HP < MaxHealth).Subscribe(_ => healthBar.gameObject.SetActive(true)).AddTo(gameObject);
        Observable.EveryUpdate().Where(_ => HP <= 0).Subscribe(_ => isDestroyed = true).AddTo(gameObject);
    }
}
