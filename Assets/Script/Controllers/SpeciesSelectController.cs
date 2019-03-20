using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeciesSelectController : MonoBehaviour {
    private int index;
    public int Index {
        get { return index; }
        set {
            index = value;
            OnChangeSpecies();
        }
    }
    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.MenuScene;

    private void OnEnable() {
        Index = 0;
    }

    public void ToggleModal(bool toggle) {
        gameObject.SetActive(toggle);
    }

    private void OnChangeSpecies() {

    }

    public void UnableToSelect() {
        Modal.instantiate("아직 준비중입니다.", Modal.Type.CHECK);
    }

    public void OnConfirmBtn() {
        AccountManager.Instance.selectNumber = AccountManager.Instance.decks.Count;
        GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
        gsm.startScene(sceneState, GameSceneManager.SceneState.DeckSettingScene);
        DeckSettingController.prevData = null;
    }
} 
