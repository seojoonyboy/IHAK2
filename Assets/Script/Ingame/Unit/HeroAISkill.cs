using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class HeroAI : UnitAI {
    //영웅들의 스킬 관련 함수

    public void Lakan_bite() {
        StartCoroutine("Find_Lakan_bite_Target");
    }

    IEnumerator Find_Lakan_bite_Target() {
        GameObject targetObject = null;
        while (!targetObject) {
            List<HeroAI> heroes = new List<HeroAI>();
            GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
            
            int layerToGive = LayertoGive(true);
            float tempLegnth = 100;
            HeroAI skillTargetHero = null;
            UnitAI skillTargetUnit = null;
            MonsterAI skillTargetMonster = null;
            for (int i = 0; i < units.Length; i++) {
                if (units[i].layer != layerToGive) continue;
                float length = Vector3.Distance(units[i].transform.position, transform.position);
                if (length > 100f) continue;
                if (units[i].GetComponent<HeroAI>() == null && skillTargetHero != null) continue;
                if (length < tempLegnth) {
                    if (units[i].GetComponent<HeroAI>())
                        skillTargetHero = units[i].GetComponent<HeroAI>();
                    else if(units[i].GetComponent<MinionAI>())
                        skillTargetUnit = units[i].GetComponent<UnitAI>();
                    else
                        skillTargetMonster = units[i].GetComponent<MonsterAI>();
                    tempLegnth = length;
                    targetObject = units[i];
                }
            }
            //yield return new WaitForSeconds(0.5f);
            yield return null;
        }
        targetUnit = targetObject.transform;
        moveSpeed = moveSpeed * 3;
        StartCoroutine("Lakan_bite_Action");
    }

    IEnumerator Lakan_bite_Action() {
        float length = Vector3.Distance(targetUnit.transform.position, transform.position);
        while (length > attackRange) {
            length = Vector3.Distance(targetUnit.transform.position, transform.position);
            yield return null;
        }
        if(targetUnit.GetComponent<UnitAI>())
            targetUnit.GetComponent<UnitAI>().damaged(power * 3, transform);
        else if(targetUnit.GetComponent<MonsterAI>())
            targetUnit.GetComponent<MonsterAI>().damaged(power * 3);
        moveSpeed = moveSpeed / 3;
        SkillFinish();
    }


    public void Wimp_evilmind() {
        List<HeroAI> heroes = new List<HeroAI>();
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        int layerToGive = LayertoGive(true);
        for (int i = 0; i < units.Length; i++) {
            if (units[i].layer == layerToGive) continue;
            float length = Vector3.Distance(units[i].transform.position, transform.position);
            if (length > 45f) continue;
            if (units[i].GetComponent<Buff_evilmind>()) continue;
            units[i].AddComponent<Buff_evilmind>();
        }
    }

    public void Shell_humantorch() {
        weaponSkill = Shell_attack;       
    }

    public void Shell_attack() {
        if(targetUnit != null) {
            if (targetUnit.GetComponent<IngameBuilding>()) return;
            Debuff_Humantorch debuff = targetUnit.GetComponent<Debuff_Humantorch>();
            if (debuff != null) Destroy(targetUnit.GetComponent<Debuff_Humantorch>());
            targetUnit.gameObject.AddComponent<Debuff_Humantorch>().SetFlameDamage(power);
        }
    }

    public void Rex_satiation() {
        StartCoroutine("Find_Rex_satiation_Target");
    }

    IEnumerator Find_Rex_satiation_Target() {
        bool skillActed = false;
        int drainHp = 0;
        while (!skillActed) {
            List<HeroAI> heroes = new List<HeroAI>();
            GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
            int layerToGive = LayertoGive(true);
            for (int i = 0; i < units.Length; i++) {
                if (units[i].layer != layerToGive) continue;
                float length = Vector3.Distance(units[i].transform.position, transform.position);
                if (length > 30f) continue;
                if (units[i].GetComponent<HeroAI>() == null) {
                    drainHp += 5;
                    if(units[i].GetComponent<MonsterAI>())
                        units[i].GetComponent<MonsterAI>().damaged(power * 0.8f);
                    else
                        units[i].GetComponent<UnitAI>().damaged(power * 0.8f, transform);
                    skillActed = true;
                }
                else {
                    drainHp += 20;
                    drainHp += 5;
                    units[i].GetComponent<HeroAI>().damaged(power * 0.8f, transform);
                    skillActed = true;
                }
            }
            yield return null;
        }
        Healed(drainHp);
        SkillFinish();
    }


    /// <summary>
    /// 시간 제한이 없고 특수 조건 (예 : 스킬 완료시)이 걸린 스킬일 경우
    /// 해당 함수를 추가해주시면 됩니다.
    /// </summary>
    private void SkillFinish() {
        setState(skillState.COOLING);
    }
}
