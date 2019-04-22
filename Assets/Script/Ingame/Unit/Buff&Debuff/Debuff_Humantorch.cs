using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debuff_Humantorch : MonoBehaviour {

    private UnitAI unitIdentity;
    private MonsterAI monsterIdentity;
    private float flameDamage;
    private bool inMonster = false;
    float time = 0;
    int count = 0;
    

    public void SetFlameDamage(float damage) {
        flameDamage = damage;
    }

    private void Start() {
        if (gameObject.GetComponent<UnitAI>() != null)
            unitIdentity = gameObject.GetComponent<UnitAI>();
        else {
            inMonster = true;
            monsterIdentity = gameObject.GetComponent<MonsterAI>();
        }
        
    }

    // Update is called once per frame
    void Update () {
        time += Time.deltaTime;
        if(time >= 1.0f) {
            if(!inMonster) unitIdentity.damaged(flameDamage);
            else monsterIdentity.Damage(flameDamage);
            time = 0;
            count++;
        }
        if (count == 3)
            Destroy(this);
	}
}
