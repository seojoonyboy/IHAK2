using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff_evilmind : MonoBehaviour {

    private UnitAI unitIdentity;
    float basePower;
    float time = 0;

    // Use this for initialization
    void Start () {
        unitIdentity = gameObject.GetComponent<UnitAI>();
        basePower = unitIdentity.power;
        unitIdentity.power = basePower * 1.1f;
    }

    private void Update() {
        time += Time.deltaTime;
        if (time > 6.0f) {
            unitIdentity.power = basePower;
            Destroy(this);
        }
    }
}
