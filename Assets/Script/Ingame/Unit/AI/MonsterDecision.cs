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
        List<GameObject> targets = new List<GameObject>();
        if(tower.GetType() == typeof(CreepStation)) {
            targets = ((CreepStation)tower).targets;
        }
        if (targets != null && targets.Count != 0) {
            foreach (GameObject target in targets) {
                if (target == null) {
                    targets.Remove(target);
                }
            }
            int rndNum = Random.Range(0, targets.Count - 1);
            controller.chaseTarget = targets[rndNum].transform;

            Destroy(controller.GetComponent<Patrol>());
            return true;
        }
        return false;
    }
}