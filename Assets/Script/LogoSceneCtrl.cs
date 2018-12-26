using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoSceneCtrl : MonoBehaviour {

    MainViewController.SceneState sceneState = MainViewController.SceneState.LogoScene;

    public void startButton() {
        MainViewController mvc = FindObjectOfType<MainViewController>();
        mvc.startScene(sceneState.ToString(), MainViewController.SceneState.MenuScene);
    }
}
