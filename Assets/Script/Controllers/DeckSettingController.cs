using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSettingController : MonoBehaviour {

    GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.DeckSettingScene;
	

    public void returnButton() {
        GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
        Destroy(GameObject.FindGameObjectWithTag("TileGroup"));
        gsm.startScene(sceneState.ToString(), GameSceneManager.SceneState.MenuScene);
    }
}
