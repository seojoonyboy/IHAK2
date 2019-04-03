using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class HeroAI : UnitAI {
	//영웅들의 SP 관련 & 스킬 관련 함수

    public void Lakan_bite() {
        List<HeroAI> heroes = new List<HeroAI>();
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        int layerToGive = gameObject.layer == myLayer ? enemyLayer : myLayer;
        float targetLegnth = 100;
        HeroAI skillTargetHero = null;
        UnitAI skillTargetUnit = null;
        GameObject targetObject = null; ;
        for (int i = 0; i < units.Length; i++) {
            if (units[i].layer == layerToGive) continue;
            float length = Vector3.Distance(units[i].transform.position, transform.position);
            if (length > 36f) continue;
            if (units[i].GetComponent<HeroAI>() == null && skillTargetHero != null) continue;
            if (length < targetLegnth) {
                if (units[i].GetComponent<HeroAI>())
                    skillTargetHero = units[i].GetComponent<HeroAI>();
                else
                    skillTargetUnit = units[i].GetComponent<UnitAI>();
                targetLegnth = length;
                targetObject = units[i];
            }
        }

        if(targetObject != null) {
            targetUnit = targetObject.GetComponent<UnitAI>();
            moveSpeed = moveSpeed * 3;
        }
    }

    IEnumerator Lakan_bite_Action() {
        float length = Vector3.Distance(targetUnit.transform.position, transform.position);
        while (length > attackRange) {
            length = Vector3.Distance(targetUnit.transform.position, transform.position);
        }
        targetUnit.damaged(power * 3);
        moveSpeed = moveSpeed / 3;
        return null;
    }


    public void Wimp_evilmind() {
        List<HeroAI> heroes = new List<HeroAI>();
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        int layerToGive = gameObject.layer == myLayer ? enemyLayer : myLayer;
        for (int i = 0; i < units.Length; i++) {
            if (units[i].layer != layerToGive) continue;
            float length = Vector3.Distance(units[i].transform.position, transform.position);
            if (length > 45f) continue;
            if(units[i].GetComponent<Buff_evilmind>())
                Destroy(units[i].GetComponent<Buff_evilmind>());
            units[i].AddComponent<Buff_evilmind>();
        }
    }

    public void Shell_humantorch() {
        if (targetUnit.GetComponent<Debuff_Humantorch>())
            Destroy(targetUnit.GetComponent<Debuff_Humantorch>());
        if (targetUnit.GetComponent<UnitAI>()) {
            targetUnit.gameObject.AddComponent<Debuff_Humantorch>();
            targetUnit.gameObject.AddComponent<Debuff_Humantorch>().SetFlameDamage(power);
        }
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
