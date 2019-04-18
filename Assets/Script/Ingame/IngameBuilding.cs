using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameBuilding : MonoBehaviour {

    public int buildingHp;
    public int maxHp;

    public void damaged(float damage) {
        buildingHp -= Mathf.FloorToInt(damage);
        
    }

    public virtual void TakeDamage(float amount) { }
    
}
