using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FowardHQ : IngameBuilding {

    public CircleCollider2D effectRange;
    [SerializeField] [ReadOnly] private bool isDestroyed = false;

    public bool IsDestroyed {
        get { return isDestroyed; }
        set { isDestroyed = true; }
    }
    // Use this for initialization
    void Start() {
        maxHp = 300;
        buildingHp = maxHp;
        effectRange = transform.parent.GetComponent<CircleCollider2D>();
    }

    private void setRange(float amount) {
        effectRange.radius = amount;
    }

}
