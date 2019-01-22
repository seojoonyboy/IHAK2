using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameScoreManager : Singleton<IngameScoreManager> {  
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

    public int playerScore = 0;

    // Use this for initialization
    public void AddScore(int num, ScoreType type, int gametime = 0) {
        switch (type) {
            case ScoreType.Product: //생산량을 num으로 받음
                playerScore += num * 4;
                break;
            case ScoreType.ActiveCard:
                playerScore += (100 * num);
                break;
            case ScoreType.Attack:
                playerScore += num * 8;
                break;
            case ScoreType.DestroyUnit:
                playerScore += num * 150;
                break;
            case ScoreType.DestroyBuilding:
                playerScore += num * 300;
                break;
            case ScoreType.DestroyCity:
                playerScore += num * gametime + 300;
                break;


            case ScoreType.FoodFirst:
                playerScore += 4000;
                break;
            case ScoreType.GoldFirst:
                playerScore += 4000;
                break;
            case ScoreType.DamageFirst:
                playerScore += 4000;
                break;
            case ScoreType.HealthFirst:
                playerScore += 4000;
                break;
            case ScoreType.EnvFirst:
                playerScore += num * 25;
                break;
            case ScoreType.SpecialTile:
                playerScore += playerScore * (1 + num);
                break;
        }
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }
}
