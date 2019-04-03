using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScaryOracle : MonoBehaviour {
    public int percentage;

    void Start() {
        GetComponent<UnitAI>().ChangeSpeedByPercentage(-percentage);
    }

    void OnDestroy() {
        GetComponent<UnitAI>().ResetSpeedPercentage();
    }
}
