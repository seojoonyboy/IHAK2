using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;

public class FieldHospital : IngameBuilding {

    public CircleCollider2D effectRange;
    public float delayTime = 1f;
    public float time = 0f;
    public int count = 0;
    GameObject target;
    // Use this for initialization
    void Start () {
        maxHp = 300;
        buildingHp = maxHp;
        effectRange = transform.gameObject.GetComponent<CircleCollider2D>();
    }

    private void setRange(float amount) {
        effectRange.radius = amount;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.name.CompareTo("Skeleton") != 0 && collision.gameObject.layer == 10) {
            collision.gameObject.AddComponent<Heal>();
            collision.GetComponent<Heal>().delayTime = delayTime;
        }

        if(collision.gameObject.layer != 10) {
            Heal heal = collision.gameObject.GetComponent<Heal>();
            if(heal != null) {
                Destroy(heal);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.name.CompareTo("Skeleton") != 0 && collision.gameObject.layer == 10) {
            Heal heal = collision.gameObject.GetComponent<Heal>();
            if (heal != null)
                Destroy(heal);
        }
    }



}
