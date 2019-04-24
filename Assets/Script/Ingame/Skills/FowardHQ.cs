using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;

public class FowardHQ : IngameBuilding {

    public CircleCollider2D effectRange;
    private int damage;
    private float atkTime;

    [SerializeField] private Transform enemy;
    
    [SerializeField] bool isAttacking = false;
    [SerializeField] bool isDestroyed = false;

    [SerializeField] private float time;
    [SerializeField] private GameObject arrow;

    public bool clicked;
    public GameObject select;
    public Toggle toggle;

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
    /*
    public void temp() {
        var stream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));
        var selectStream = stream.Where(_=> select == null).Subscribe(_ => select = Detect()).AddTo(this);

        var temp = toggle.OnPointerDownAsObservable();
        temp.Buffer(stream.Throttle(System.TimeSpan.FromMilliseconds(200))).Where(x => x.Count >= 2).Subscribe(_ => Debug.Log("Do Someting"));
       
        stream.Buffer(stream.Throttle(System.TimeSpan.FromMilliseconds(200))).Where(x => x.Count >= 2).Where(_=>select == Detect()).Subscribe(_ => { Debug.Log("더블!"); select = null; });
        stream.Buffer(stream.Throttle(System.TimeSpan.FromMilliseconds(200))).Where(x => x.Count >= 2).Subscribe(_ => select = null);
        stream.Where(_ => select != null).Buffer(stream.Throttle(System.TimeSpan.FromMilliseconds(200))).Where(x => x.Count < 2).Subscribe(_ => { Debug.Log("시간초과!"); select = null; });
    }

    public GameObject Detect() {
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(position, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null) {
            return hit.collider.gameObject;
        }
        return null;
    }
    */

}
