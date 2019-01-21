using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameScoreManager : MonoBehaviour {
    public enum ScoreType{
        Product,
        ActiveCard,
        Attack,
        DestroyUnit,
        DestroyBuilding,
        DestroyCity,

        FoodFirst,
        GoldFirst,
        EnvFirst,
        DamageFirst,
        HealthFirst,
        SpecialTile
    }

    private int playerScore = 0;

	// Use this for initialization
	public static void Instantiate(int rare, ScoreType type) {
        switch (type) {
            case ScoreType.Product:
                break;
            case ScoreType.ActiveCard:
                break;
            case ScoreType.Attack:
                break;
            case ScoreType.DestroyUnit:
                break;
            case ScoreType.DestroyBuilding:
                break;
            case ScoreType.DestroyCity:
                break;


            case ScoreType.FoodFirst:
                break;
            case ScoreType.GoldFirst:
                break;
            case ScoreType.DamageFirst:
                break;
            case ScoreType.HealthFirst:
                break;
            case ScoreType.SpecialTile:
                break;
            case ScoreType.EnvFirst:
                break;
        }
    }
}
