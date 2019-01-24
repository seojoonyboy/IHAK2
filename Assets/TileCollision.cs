using UnityEngine;
using System.Collections;

public class TileCollision : MonoBehaviour {

    public bool check;
    public int count = 0;

    private void Start() {
    }
    /*
    public void OnTriggerEnter2D(Collider2D coll) {
        
        if(coll.gameObject.layer == LayerMask.NameToLayer("PlayerUnit"))
            check = true;
            

        if (coll.gameObject.tag == "Unit")
            check = true;
    }

    public void OnTriggerStay2D(Collider2D coll) {

        if (coll.gameObject.tag == "Unit") {
            check = true;
        }

        if (coll.gameObject == null)
            check = false;
    }

    public void OnTriggerExit2D(Collider2D coll) {
        
        if (coll.gameObject.layer == LayerMask.NameToLayer("PlayerUnit"))
            check = false;
            
        if (coll.gameObject.tag == "Unit")
            check = true;
    }
    */
}
