using AI_submodule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적을 감지하였을 때 처리를 위한 State
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/ActiveState")]
public class DetectStateDecision : Decision {
    public override bool Decide(StateController controller) {
        bool targetConfirmed = Chase(controller);
        return targetConfirmed;
    }

    private bool Chase(StateController controller) {
        controller.GetComponent<MonsterAI>().monsterSpine.Move();
        var tower = controller.GetComponent<MonsterAI>().tower;
        if (controller.chaseTarget != null) {
            return true;
        }
        else {
            if (tower.targets.Count != 0) {
                int rndNum = Random.Range(0, tower.targets.Count - 1);
                controller.chaseTarget = tower.targets[rndNum].transform;
                return true;
            }
            else {
                if(controller.GetComponent<MoveToAttack>() != null) {
                    Destroy(controller.GetComponent<MoveToAttack>());
                }
                return false;
            }
        }
    }
}