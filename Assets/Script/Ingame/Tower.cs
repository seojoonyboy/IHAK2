using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour {


    private void OnTriggerEnter2D(Collider2D collision) {
        Debug.Log("트리거");
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        Debug.Log("콜라이더");
    }



}
