using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        Health,
        SpecialTile
    }

    public int playerScore = 0;
    public int dummyScore = 50000;

    public Dictionary<string, int> scoreList;


    /// <summary>
	/// 점수 연산 및 자동 대입
	/// </summary>
	/// <param name="num">공식에 적용되는 배수</param>
	/// <param name="type">ScoreType.종류</param>
	/// <param name="gametime">게임 시간</param>
    // Use this for initialization
    public void AddScore(int num, ScoreType type, int gametime = 0, int rarity = 0) {
        switch (type) {
            case ScoreType.Product: //생산량을 num으로 받음
                playerScore += num;
                break;
            case ScoreType.ActiveCard:
                playerScore += 20 * (1 + rarity + (num / 2));
                break;
            case ScoreType.Attack:
                playerScore += num * 2;
                break;
            case ScoreType.DestroyUnit:
                playerScore += 70 * (1 + rarity + (num / 2));
                break;
            case ScoreType.DestroyBuilding:
                playerScore += 100 * (1 + rarity + (num / 2));
                break;
            case ScoreType.DestroyCity:
                playerScore += 60 * (rarity + gametime + (num / 2));
                break;


            case ScoreType.FoodFirst:
                playerScore += 3000;
                break;
            case ScoreType.GoldFirst:
                playerScore += 3000;
                break;
            case ScoreType.DamageFirst:
                playerScore += 3000;
                break;
            case ScoreType.EnvFirst:
                playerScore += 3000;
                break;
            case ScoreType.Health:
                playerScore += num;
                break;
            case ScoreType.SpecialTile:
                playerScore += playerScore * (1 + num);
                break;
        }
    }

    public void SortScore() {
        Dictionary<string, int> tempList = new Dictionary<string, int>();
        tempList.Add(AccountManager.Instance.userInfos.nickname, playerScore);
        tempList.Add("Dummy", dummyScore);

        scoreList = tempList.OrderByDescending(num => num.Value).ToDictionary(score => score.Key, score => score.Value);
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }
}
