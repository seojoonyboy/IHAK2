using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class UnitAI : MonoBehaviour {

    [SerializeField] Dictionary<string, Buff> buffList = new Dictionary<string, Buff>();

    public void AddBuff(string name, Buff buff) {
        buffList.Add(name, buff);
    }

    public void RemoveBuff(string name) {
        buffList.Remove(name);
    }
    
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
