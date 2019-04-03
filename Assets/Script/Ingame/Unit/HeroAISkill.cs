using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class HeroAI : UnitAI {
	//영웅들의 SP 관련 & 스킬 관련 함수

    public void Lakan_bite() {
        
    }

    public void Wimp_evilmind() {

    }

    public void Shell_humantorch() {

    }

    public void Rex_fedup(float attackPower) {
        int drainHp = 0;
        List<HeroAI> heroes = new List<HeroAI>();
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        int layerToGive = gameObject.layer == myLayer ? enemyLayer : myLayer;
        for (int i = 0; i < units.Length; i++) {
            if (units[i].layer == layerToGive) continue;
            float length = Vector3.Distance(units[i].transform.position, transform.position);
            if (length > 30f) continue;
            if (units[i].GetComponent<HeroAI>() == null) {
                drainHp += 5;
                units[i].GetComponent<UnitAI>().damaged(power * 0.8f);
            }
            else {
                drainHp += 20;
                drainHp += 5;
                units[i].GetComponent<HeroAI>().damaged(power * 0.8f);
            }
        }
    }
}
