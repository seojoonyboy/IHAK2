using AI_submodule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/ActiveState")]
public class ActiveStateDecision : Decision {
    public override bool Decide(StateController controller) {
        var tower = controller.GetComponent<MonsterAI>().tower;
        if (controller.chaseTarget != null) {
            return true;
        }
        return false;
    }
}