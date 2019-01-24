using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class BasicController : MonoBehaviour {

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.MenuScene;

    [SerializeField] Button startBattleButton;

    // Use this for initialization
    void Start () {
        if(AccountManager.Instance.decks.Count == 0) 
            startBattleButton.interactable = false;
        startBattleButton.OnClickAsObservable().Subscribe(_ => StartBattle());
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void StartBattle() {
        GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
        gsm.startScene(sceneState, GameSceneManager.SceneState.IngameScene);
    }
}
