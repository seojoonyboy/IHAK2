using AI;
using AI_submodule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Patrol")]
public class PatrolAction : Action {
    public override void Act(StateController controller) {
        Patrol(controller);
    }

    public override void TimeReset(StateController controller) { }

    private void Patrol(StateController controller) {
        if (controller.gameObject.GetComponent<Patrol>() == null) {
            controller.gameObject
            .AddComponent<Patrol>()
            .Init(controller.wayPointList);
        }
    }
}