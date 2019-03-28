using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;

public class MinionAI : UnitAI {
	[SerializeField] private Unit unit;

    public override int CalPower() {
        return Mathf.RoundToInt(power);
    }

	private void Init() {
		if (healthBar != null) return;
        healthBar = transform.Find("UnitBar/HP");
		unitSpine = GetComponentInChildren<UnitSpine>();
	}

	public override void SetUnitData(ActiveCard card) {
		Init();
		moveSpeed = unit.moveSpeed;
		attackSpeed = unit.attackSpeed;
		attackRange = unit.attackRange;
        power = unit.power;
		SetMaxHP();
		calculateHealthBar();
	}

	private void SetMaxHP() {
        maxHealth = unit.hitPoint;
    }
	public override void DestoryEnemy() {
		Destroy(gameObject);
	}
}
