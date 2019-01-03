using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;

public class DeckSettingController : MonoBehaviour {

    GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.DeckSettingScene;
    GameSceneManager gsm;
    PlayerInfosManager playerInfosManager;
    [SerializeField]
    public GameObject deckSet;

    private void Start() {
        playerInfosManager = PlayerInfosManager.Instance;
        gsm = FindObjectOfType<GameSceneManager>();
        deckSet = GameObject.FindGameObjectWithTag("TileGroup");
    }

    public void settingButton() {
        Deck deck = new Deck();
        deck.Id = playerInfosManager.decks.Capacity;
        deck.deckData = transform.GetChild(0).GetChild(0).GetComponent<DropHandler>().deckData; // 타일 하위 오브젝트 스크립트 말고 그룹용 오브젝트 스크립트를 만들어야하나?
        deck.Name = "중세시대";

        playerInfosManager.AddDeck(deck);
        Destroy(deckSet);
        gsm.startScene(sceneState.ToString(), GameSceneManager.SceneState.MenuScene);
    }

    public void returnButton() {
        Destroy(deckSet);
        gsm.startScene(sceneState.ToString(), GameSceneManager.SceneState.MenuScene);
    }
}
