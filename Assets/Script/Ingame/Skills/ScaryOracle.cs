using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScaryOracle : Buff {
    public int percentage;

    void Start() {
        moveSpeed_percentage = percentage;
        //GetComponent<UnitAI>().ChangeSpeedByPercentage(-percentage);
    }

    void OnDestroy() {
        //GetComponent<UnitAI>().ResetSpeedPercentage();
    }
}
