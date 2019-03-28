﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;

public class MinionAI : UnitAI {
	[SerializeField] private Unit unit;
	[SerializeField] private int expPoint;

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
		InitStatic();
		moveSpeed = unit.moveSpeed;
		attackSpeed = unit.attackSpeed;
		attackRange = unit.attackRange;
        power = unit.power;
		SetMaxHP();
		health = unit.hitPoint;
		calculateHealthBar();
	}

	private void SetMaxHP() {
        maxHealth = unit.hitPoint;
    }
	public override void DestoryEnemy() {
		GiveExp();
		Destroy(gameObject);
	}

    private void GiveExp() {
        List<HeroAI> heroes = new List<HeroAI>();
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        int layerToGive = gameObject.layer == myLayer ? enemyLayer : myLayer;
        for(int i = 0 ; i < units.Length ; i++) {
            if(units[i].layer != layerToGive) continue;
            if(units[i].GetComponent<HeroAI>() == null) continue;
            float length = Vector3.Distance(units[i].transform.position, transform.position);
            if(length <= 60f) continue;
            heroes.Add(units[i].GetComponent<HeroAI>());
        }
        if(heroes.Count == 0) return;
        int exp = expPoint / heroes.Count;
		for(int i = 0 ; i < heroes.Count ; i++) heroes[i].ExpGain(exp);
    }
}