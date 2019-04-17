using AI_submodule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적을 감지하여 추격하는 State
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/AttackState")]
public class AttackStateDecision : Decision {
    public override bool Decide(StateController controller) {
        bool closeToTarget = CloseToTarget(controller);
        return closeToTarget;
    }

    private bool CloseToTarget(StateController controller) {
        if(controller.GetComponent<MoveToAttack>() != null) {
            var result = controller.GetComponent<MoveToAttack>().isCloseToTarget;
            return result;
        }
        return true;
    }
}