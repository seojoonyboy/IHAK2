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
        HeroAI hero = target.GetComponent<HeroAI>();
        UnitAI unit = target.GetComponent<UnitAI>();

        if (hero && unit == null) return;

        if(hero != null) {
            if (hero.health < 300)
                hero.health += 10;
        }
        else if(unit != null) {
            if (unit.health < 300)
                unit.health += 10;
        }       
    }


}
