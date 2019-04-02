using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class UnitAI : MonoBehaviour {
    
    public void HerbRation(int percent) {
        float healing = health * percent / 100.0f;
        health += healing;
        calculateHealthBar();
    }

    public void ScaryOracle(int percent) {
        float reduction = moveSpeed * percent / 100.0f;
        moveSpeed -= reduction;
    }
}
