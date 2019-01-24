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
    public int dummyScore = 100000;

    public Dictionary<string, int> scoreList;


    /// <summary>
	/// 점수 연산 및 자동 대입
	/// </summary>
	/// <param name="num">공식에 적용되는 배수</param>
	/// <param name="type">ScoreType.종류</param>
	/// <param name="gametime">게임 시간</param>
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
            case ScoreType.EnvFirst:
                playerScore += 4000;
                break;
            case ScoreType.Health:
                playerScore += num * 1;
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
