using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DataModules;

public partial class HeroAI : UnitAI {
    public enum skillState {
        COOLING,
        SKILLING,
        WAITING_DONE
    }

    private timeUpdate skillUpdate;
    private float coolTime;
    private float activeSkillCoolTime;
    private float skillUsingTime;
    
    private UnityAction skillActivate;
    private UnityAction weaponSkill;
    
    private void setState(skillState state) {
        Debug.Log("setState");
        skillUpdate = null;
        coolTime = 0f;
        switch (state) {
            case skillState.COOLING:
                skillUpdate = CoolTimeUpdate;
                break;
            case skillState.SKILLING:
                skillUpdate = UsingSkillUpdate;
                break;
            case skillState.WAITING_DONE:
                skillUpdate = noneUpdate;
                break;
        }
    }

    void Update() {
        update(Time.deltaTime);
        skillUpdate(Time.deltaTime);
    }

    void CoolTimeUpdate(float time) {
        coolTime += time;
        cooltimeBar.localScale = new Vector3(coolTime / activeSkillCoolTime, cooltimeBar.transform.localScale.y, 0);
        if (coolTime < activeSkillCoolTime) return;
        skillActivate();
        setState(skillState.SKILLING);

    }

    void UsingSkillUpdate(float time) {
        coolTime += time;
        if(coolTime < skillUsingTime) return;
        if(skillUsingTime < 0f) {
            setState(skillState.WAITING_DONE);
            return;
        }
        weaponSkill = null;
        setState(skillState.COOLING);
    }

    void noneUpdate(float time) {}

    private void FindUnitSkill(UnitSkill unitSkill) {
        if(unitSkill == null) {
            Debug.LogWarning(string.Format("{0}이 스킬을 가지고 있지 않습니다.", name));
            setState(skillState.WAITING_DONE);
            return;
        }
        switch(unitSkill.method.methodName) {
            case "bite" : //라칸 물어뜯기
                SetSkill(Lakan_bite, 3f, -1f);
                break;
            case "arsonist" :   //쉘 방화범
                SetSkill(Shell_humantorch, 3f, 5f);
                break;
            //TODO : 서버에서 이름 오면 재세팅
            case "evil_mind" : //윔프 악한마음 
                SetSkill(Wimp_evilmind, 9f, 6f);
                break;
            //TODO : 서버에서 이름 오면 재세팅
            case "satiation": //렉스 포식
                SetSkill(() => Rex_satiation(power), 7f, -1f);
                break;
            default :
                Debug.LogWarning(string.Format("{0}이 스킬을 가지고 있지 않습니다.", name));
                SetSkill(() => noneUpdate(0f), 0f, 0f);
                setState(skillState.WAITING_DONE);
                break;
        }
    }

    private void SetSkill(UnityAction skill, float coolTime, float usingTime) {
        skillActivate = skill;
        activeSkillCoolTime = coolTime;
        skillUsingTime = usingTime;
    }
}
