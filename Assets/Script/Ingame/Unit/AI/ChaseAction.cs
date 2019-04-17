using AI;
using AI_submodule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Chase")]
public class ChaseAction : Action {
    public override void Act(StateController controller) {
        Chase(controller);
    }

    private void Chase(StateController controller) {
        if (controller.gameObject.GetComponent<MoveToAttack>() == null) {
            controller.gameObject
            .AddComponent<MoveToAttack>()
            .Init(controller);
        }
    }
}