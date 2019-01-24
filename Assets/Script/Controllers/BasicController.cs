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
        startBattleButton.OnClickAsObservable().Subscribe(_ => StartBattle());
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void StartBattle() {
        if(AccountManager.Instance.decks.Count == 0)
            Modal.instantiate("덱이 존재하지 않습니다.\n덱을 하나 만들어주세요.", Modal.Type.CHECK);
        GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
        gsm.startScene(sceneState, GameSceneManager.SceneState.IngameScene);
    }
}
