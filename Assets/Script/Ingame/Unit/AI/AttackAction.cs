using AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Attack")]
public class AttackAction : Action {
    private float time;
    private float interval = 5;
    public override void Act(StateController controller) {
        Attack(controller);
    }

    private void Attack(StateController controller) {
        var target = controller.chaseTarget;
        if(time == 0) {
            time = controller.time;
        }
        
        if (controller.time - time > interval) {
            if (target.GetComponent<UnitAI>() != null) {
                controller.GetComponent<MonsterAI>().monsterSpine.Attack();
            }
            Debug.Log("공격!");
            time = 0;
        }
    }
}