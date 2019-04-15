using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FieldHospital : IngameBuilding {

    public CircleCollider2D effectRange;
    // Use this for initialization
    void Start () {
        maxHp = 300;
        buildingHp = maxHp;
    }

    private void setRange(float amount) {
        effectRange.radius = amount;
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if(collision.gameObject.layer == 10) {
            if (collision.name.CompareTo("Skeleton") == 0) return;
            healUnit(collision.transform.gameObject);
            Debug.Log(collision.name);
        }
    }

    private void healUnit(GameObject target) {
        if (target.layer != 10) return;
        UnitAI unit = target.GetComponent<UnitAI>();

        if (unit == null) return;



        if (unit.health < 300)
            unit.health += 10;
             
    }


}
