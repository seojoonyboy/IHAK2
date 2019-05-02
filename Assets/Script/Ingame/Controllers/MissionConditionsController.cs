using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionConditionsController : SerializedMonoBehaviour {
    IngameSceneEventHandler eventHandler;
    AccountManager accountManager;
    public List<ConditionSet> conditions;
    public List<ConditionSet> oppenentConditions;

    void Awake() {
        eventHandler = IngameSceneEventHandler.Instance;
        accountManager = AccountManager.Instance;
    }

    void Start() {
        conditions = new List<ConditionSet>();
        oppenentConditions = new List<ConditionSet>();

        DataModules.Conditions[] condData = accountManager.mission.PlayerConditions;
        foreach(DataModules.Conditions condition in condData) {
            ParseConditions(condition, true);
        }

        condData = accountManager.mission.opponentConditions;
        foreach(DataModules.Conditions condition in condData) {
            ParseConditions(condition, false);
        }
    }

    private void ParseConditions(DataModules.Conditions conditions, bool isForPlayer) {
        try {
            Conditions cond = (Conditions)Enum.Parse(typeof(Conditions), conditions.methodName);
            int[] args = ParseArgs(conditions.args);
            if (isForPlayer) {
                this.conditions.Add(new ConditionSet(cond, args));
            }
            else {
                oppenentConditions.Add(new ConditionSet(cond, args));
            }
        }
        catch (ArgumentException ex) {
            Debug.LogError("Mission 조건 처리 관련 에러 : " + ex.ToString());
        }
    }

    private int[] ParseArgs(string[] data) {
        int[] args = new int[data.Length];
        for(int i=0; i<data.Length; i++) {
            int.TryParse(data[i], out args[i]);
        }
        return args;
    }
}

[System.Serializable]
public class ConditionSet {
    public Conditions condition;
    public int[] args;

    public ConditionSet(Conditions condition, int[] args) {
        this.condition = condition;
        this.args = args;
    }
}

[Flags]
public enum Conditions {
    exp_add,
    cooltime_fix,
    hero_cooltime_fix,
    geojeom_monster_count,
    hero_buff
};