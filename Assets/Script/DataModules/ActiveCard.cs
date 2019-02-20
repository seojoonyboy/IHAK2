using DataModules;
using UnityEngine;

public class ActiveCardInfo : MonoBehaviour {
    public ActiveCard data;
}

[System.Serializable]
public class ActiveCard {
    public GameObject parentBuilding;
    public Unit unit;
    public Skill skill;
}