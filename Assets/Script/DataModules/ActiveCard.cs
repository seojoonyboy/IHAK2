using DataModules;
using UnityEngine;

public class ActiveCardInfo : MonoBehaviour {
    public ActiveCard data;
}

[System.Serializable]
public class BaseSpec {
    public Unit unit;
    public Skill skill;
}

[System.Serializable]
public class Ev {
    public int lv;
    public int exp;
    public int hp;
    public int time;
}