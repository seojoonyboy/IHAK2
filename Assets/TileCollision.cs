using UnityEngine;

public class TileCollision : MonoBehaviour {

    public bool check;

    public void OnTriggerEnter2D(Collider2D coll) {
        if(coll.gameObject.layer == LayerMask.NameToLayer("PlayerUnit"))
            check = true;
    }

    public void OnTriggerStay2D(Collider2D coll) {
        if (coll.gameObject.layer == LayerMask.NameToLayer("PlayerUnit"))
            check = true;
    }

    public void OnTriggerExit2D(Collider2D coll) {
        if (coll.gameObject.layer == LayerMask.NameToLayer("PlayerUnit"))
            check = false;
    }
}
