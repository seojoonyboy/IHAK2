using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;

public class MinionAI : UnitAI {
    [SerializeField] private Unit unit;
    [SerializeField] private int expPoint;

    public override int CalPower() {
        return Mathf.RoundToInt(power);
    }

    public override void Init(object card) {
        healthBar = transform.Find("UnitBar/HP");
        unitSpine = GetComponentInChildren<UnitSpine>();
    }

    public override void Init(object card, GameObject cardObj) {
        Init(card);
        InitStatic();
        moveSpeed = unit.moveSpeed;
        attackSpeed = unit.attackSpeed;
        attackRange = unit.attackRange;
        power = unit.attackPower;
        SetMaxHP();
        HP = unit.hitPoint;
        CalculateHealthBar();
        SetColliderData();
    }

    public void SetMinionData(ActiveCard heroCard, bool levelup = false) {
        Init(heroCard);
        InitStatic();
        moveSpeed = heroCard.baseSpec.unit.moveSpeed;
        float heroBonus = ((heroCard.baseSpec.unit.minion.capabilityArgs[0]) + (heroCard.baseSpec.unit.minion.capabilityArgs[1] * heroCard.ev.lv)) / 100.0f;
        attackSpeed = unit.attackSpeed * heroBonus;
        attackRange = unit.attackRange * heroBonus;
        power = unit.attackPower * heroBonus;
        MaxHealth = unit.hitPoint * heroBonus;
        if(!levelup)
            HP = MaxHealth;
        CalculateHealthBar();
        SetColliderData();
    }

    private void SetMaxHP() {
        MaxHealth = unit.hitPoint;
    }
    public override void Die() {
        GiveExp();
        playerController.DieEffect(this);   //사망시 패시브 효과 처리
        Destroy(gameObject);
        myGroup.UnitDead();
    }

    private void GiveExp() {
        List<HeroAI> heroes = new List<HeroAI>();
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        int layerToGive = LayertoGive(true);
        for (int i = 0; i < units.Length; i++) {
            if (units[i].layer != layerToGive) continue;
            if (units[i].GetComponent<HeroAI>() == null) continue;
            float length = Vector3.Distance(units[i].transform.position, transform.position);
            if (length > 60f) continue;
            heroes.Add(units[i].GetComponent<HeroAI>());
        }
        if (heroes.Count == 0) return;
        int exp = expPoint / heroes.Count;
        for (int i = 0; i < heroes.Count; i++) heroes[i].ExpGain(exp);
    }

    public override void ResetStat() {
        moveSpeed = unit.moveSpeed;
        attackSpeed = unit.attackSpeed;
        attackRange = unit.attackRange;
        power = unit.attackPower;
        MaxHealth = unit.hitPoint;
    }
}
