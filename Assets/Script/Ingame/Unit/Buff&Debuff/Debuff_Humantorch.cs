using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debuff_Humantorch : MonoBehaviour {

    private UnitAI unitIdentity;
    private float flameDamage;
    float time = 0;
    int count = 0;

    public void SetFlameDamage(float damage) {
        flameDamage = damage;
    }

    private void Start() {
        unitIdentity = gameObject.GetComponent<UnitAI>();
    }

    // Update is called once per frame
    void Update () {
        time += Time.deltaTime;
        if(time >= 1.0f) {
            unitIdentity.damaged(flameDamage);
            time = 0;
            count++;
        }
        if (count == 3)
            Destroy(this);
	}
}
