using AI;
using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Attack")]
public class AttackAction : Action {
    public override void Act(StateController controller) {
        Attack(controller);
    }

    public override void TimeReset(StateController controller) { }

    private void Attack(StateController controller) {
        var target = controller.chaseTarget;
        if (controller.GetComponent<Timer>() == null) return;
        if (controller.time - controller.GetComponent<Timer>().time > controller.GetComponent<MonsterAI>().data.attackSpeed) {
            if (target.GetComponent<UnitAI>() != null) {
                controller.GetComponent<MonsterAI>().monsterSpine.Attack();
            }
            Debug.Log("공격!");
            controller.GetComponent<Timer>().time = controller.time;
        }
    }
}