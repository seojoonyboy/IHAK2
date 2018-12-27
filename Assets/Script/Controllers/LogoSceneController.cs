using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoSceneController : MonoBehaviour {

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.LogoScene;

    public void startButton() {
        GameSceneManager mvc = FindObjectOfType<GameSceneManager>();
        mvc.startScene(sceneState.ToString(), GameSceneManager.SceneState.MenuScene);
    }
}
