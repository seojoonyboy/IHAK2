using AI_submodule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Default 상태
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/LookDecision")]
public class MonsterDecision : Decision {

    public override bool Decide(StateController controller) {
        bool targetVisible = Look(controller);
        return targetVisible;
    }

    private bool Look(StateController controller) {
        var tower = controller.GetComponent<MonsterAI>().tower;
        if (tower.targets != null && tower.targets.Count != 0) {
            foreach (GameObject target in tower.targets) {
                if (target == null) {
                    tower.targets.Remove(target);
                }
            }
            int rndNum = Random.Range(0, tower.targets.Count - 1);
            controller.chaseTarget = tower.targets[rndNum].transform;

            Destroy(controller.GetComponent<Patrol>());
            return true;
        }
        return false;
    }
}