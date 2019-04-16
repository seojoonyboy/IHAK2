using AI_submodule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/ActiveState")]
public class DetectStateDecision : Decision {
    public override bool Decide(StateController controller) {
        var tower = controller.GetComponent<MonsterAI>().tower;
        if (controller.chaseTarget != null) {
            return true;
        }
        else {
            if(tower.monsters.Count != 0) {
                int rndNum = Random.Range(0, tower.monsters.Count - 1);
                controller.chaseTarget = tower.monsters[rndNum].transform;
                return true;
            }
        }
        return false;
    }
}