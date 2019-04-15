using DataModules;
using TMPro;
using UnityEngine;

public class Tower_Detactor : IngameBuilding {
    private CircleCollider2D box;
    private int damage;
    private float atkTime;
    private Transform enemy;
    private bool isAttacking = false;
    private float time;
    [SerializeField]
    private GameObject arrow;
    //[SerializeField]
    //public int towerShellCount = 0;
    //[SerializeField]
    //public int towerMaxShell = 0;

    void Start() {
        maxHp = 300;
        buildingHp = maxHp;
        box = GetComponent<CircleCollider2D>();
        setRange(40);
        damage = 23;
        atkTime = 1.4f;
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
        if(other.gameObject.layer == 10) {
            if (isAttacking) return;
            if(other.name.CompareTo("Skeleton")== 0) return;
            isAttacking = true;
            //enemy = other.transform.parent;
            enemy = other.transform;
            Debug.Log(enemy.name);
            time = 0f;
            box.enabled = false;
        }
    }

    private void Update() {
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
        GameObject arrow = Instantiate(this.arrow, transform.position, Quaternion.identity);        
        iTween.MoveTo(arrow, enemy.position, atkTime * 0.3f);        
        Destroy(arrow, atkTime * 0.3f);
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
}
