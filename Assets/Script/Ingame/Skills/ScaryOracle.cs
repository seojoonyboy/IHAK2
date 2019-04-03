using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScaryOracle : Buff {
    public int percentage;

    void Start() {
        moveSpeed_percentage = percentage;
        GetComponent<UnitAI>().AddBuff("scary_oracle", this);
    }

    void OnDestroy() {
        GetComponent<UnitAI>().RemoveBuff("scary_oracle");
    }
}
