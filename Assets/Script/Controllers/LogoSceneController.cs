using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogoSceneController : MonoBehaviour {

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.LogoScene;

    public void pressAnyKey() {
        transform.Find("Logo").gameObject.SetActive(false);
        AccountManager.Instance.GetUserInfo();
    }

    public void startButton() {
        GameSceneManager mvc = FindObjectOfType<GameSceneManager>();
        mvc.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
    }
}
