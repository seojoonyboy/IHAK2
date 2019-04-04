using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class UnitAI : MonoBehaviour {

    [SerializeField] Dictionary<string, Buff> buffList = new Dictionary<string, Buff>();

    public void AddBuff(string name, Buff buff) {
        if (!buffList.ContainsKey(name)) {
            buffList.Add(name, buff);
            Buffering(buff);
        }
    }

    public void RemoveBuff(string name) {
        buffList.Remove(name);
        BuffReset();
    }

    private void BuffReset() {
        ResetStat();
        foreach(KeyValuePair<string, Buff> buffPair in buffList)
            Buffering(buffPair.Value);
    }

    public virtual void ResetStat() { }

    private void Buffering(Buff buff) {
        if(buff.moveSpeed_percentage != 0) 
            moveSpeed += (moveSpeed * buff.moveSpeed_percentage / 100f);
        if(buff.attackSpeed != 0)
            attackSpeed += buff.attackSpeed;
        if(buff.power != 0) 
            power += (power * buff.power / 100f);
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
