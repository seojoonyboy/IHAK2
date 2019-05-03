using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameResultManager : MonoBehaviour {
    public enum GameOverType {
        WIN,
        LOSE,
        SURVIVE,
    }

    [SerializeField] Transform rankingCalWnd;
    [SerializeField] Transform rewardWnd;

    [SerializeField] GameObject playerSlot;
    GameObject prevWindow;
    GameOverType currentType;

    public void GameOverWindow(GameOverType type) {
        gameObject.SetActive(true);
        currentType = type;
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
        StartCoroutine(CalculateRank());
    }

    IEnumerator CalculateRank() {
        IngameScoreManager.Instance.SortScore();
        int i = 0;
        foreach(var score in IngameScoreManager.Instance.scoreList) {
            yield return new WaitForSeconds(0.5f);
            GameObject slot = Instantiate(playerSlot, rankingCalWnd.transform.GetChild(1));
            slot.transform.Find("Name").GetComponent<Text>().text = score.Key;
            slot.transform.Find("Score").GetComponent<Text>().text = score.Value.ToString();
            i++;
        }
    }

    public void GotoReward() {
        IngameScoreManager.Instance.SortScore();
        prevWindow.SetActive(false);
        transform.GetChild(4).gameObject.SetActive(true);
        int playerNum = IngameScoreManager.Instance.scoreList.Count;
        int i = 0;
        foreach (var score in IngameScoreManager.Instance.scoreList) {
            rewardWnd.GetChild(i).gameObject.SetActive(true);
            rewardWnd.GetChild(i).Find("Name").GetComponent<Text>().text = score.Key;
            rewardWnd.GetChild(i).Find("Score").GetComponent<Text>().text = score.Value.ToString();
            i++;
        }

        if(currentType == GameOverType.LOSE) {
            transform.GetChild(4).Find("RestartBtn").gameObject.SetActive(false);
        }
    }
}
