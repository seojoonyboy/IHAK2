using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff_evilmind : MonoBehaviour {

    private UnitAI unitIdentity;
    float basePower;
    float time = 0;

    // Use this for initialization
    void Start () {
        Buff emBuff = new Buff();
        emBuff.power = 10;
        unitIdentity = gameObject.GetComponent<UnitAI>();
        unitIdentity.AddBuff("evil_mind", emBuff);
    }

    private void Update() {
        time += Time.deltaTime;
        if (time > 6.0f) {
            unitIdentity.RemoveBuff("evil_mind");
            Destroy(this);
        }
    }
}
