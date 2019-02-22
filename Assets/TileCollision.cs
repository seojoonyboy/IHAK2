using UnityEngine;
using System.Collections;

public class TileCollision : MonoBehaviour {

    public bool check;
    public int count = 0;

    public void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.tag == "Unit")
            count++;
    }

    public void OnTriggerStay2D(Collider2D coll) {
        if (coll.gameObject.tag == "Unit") {
            check = true;
        }
    }

    public void OnTriggerExit2D(Collider2D coll) {
        if (coll.gameObject.tag == "Unit") {
            count--;

            if(count <= 0) {
                count = 0;
                check = false;
            }
        }
    }
}
