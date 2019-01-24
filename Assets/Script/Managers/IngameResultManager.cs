using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameResultManager : MonoBehaviour {
    public enum GameOverType {
        WIN,
        LOSE,
        SURVIVE,
    }

    GameObject prevWindow;

    public void GameOverWindow(GameOverType type) {
        gameObject.SetActive(true);
        switch (type) {
            case GameOverType.WIN:
                prevWindow = transform.GetChild(0).gameObject;
                prevWindow.SetActive(true);
                break;
            case GameOverType.LOSE:
                prevWindow = transform.GetChild(1).gameObject;
                prevWindow.SetActive(true);
                break;
            case GameOverType.SURVIVE:
                prevWindow = transform.GetChild(2).gameObject;
                prevWindow.SetActive(true);
                break;
        }
    }

    public void StartRankingCal() {
        prevWindow.SetActive(false);
        prevWindow = transform.GetChild(3).gameObject;
        prevWindow.SetActive(true);
    }

    public void GotoReward() {
        prevWindow.SetActive(false);
        transform.GetChild(4).gameObject.SetActive(true);
    }
}
