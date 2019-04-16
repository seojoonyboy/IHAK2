using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;

public class FieldHospital : IngameBuilding {

    public CircleCollider2D effectRange;
    public float delayTime = 1f;

    void Start () {
        maxHp = 300;
        buildingHp = maxHp;
        effectRange = transform.parent.GetComponent<CircleCollider2D>();
    }
    



}
