using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class UnitAI : MonoBehaviour {
    
    public void HerbRation(int percent) {
        float healing = health * percent;
        health += healing;
        calculateHealthBar();
    }

    public void ScaryOracle(int percent) {
        float reduction = moveSpeed * percent;
        moveSpeed -= reduction;
    }
}
