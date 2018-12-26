using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainViewController : MonoBehaviour {

    public enum SceneState {
        None,
        LogoScene,
        MenuScene,
        SetDeck,
        InGame,
    }

    private SceneState sceneState;

    [SerializeField] GameObject loaddingWnd;

    private void Awake() {
        sceneState = SceneState.None;
    }

    // Use this for initialization
    void Start () {
        StartCoroutine(LoaddingScene(null, SceneState.LogoScene.ToString(), SceneState.LogoScene));
    }

    public void startMenuScene() {
        StartCoroutine(LoaddingScene(sceneState.ToString(), SceneState.MenuScene.ToString(), SceneState.MenuScene));
    }

    IEnumerator LoaddingScene(string remove, string load, SceneState state) {
        
        sceneState = SceneState.None;
        AsyncOperation AO;
        GameObject go = Instantiate(loaddingWnd);
        LoadingWindow _wndLoadding;
        _wndLoadding = go.GetComponent<LoadingWindow>();
        _wndLoadding.initLoaddingLogo();

        if (remove != null) {
            AO = SceneManager.UnloadSceneAsync(remove);
            while (!AO.isDone) {
                _wndLoadding.setLoaddingValue(AO.progress / 3);
                yield return null;
            }
        }
        yield return new WaitForSeconds(0.5f);
        _wndLoadding.setLoaddingValue(0.5f);
        yield return new WaitForSeconds(0.5f);
        AO = SceneManager.LoadSceneAsync(load, LoadSceneMode.Additive);
        while (!AO.isDone) {
            _wndLoadding.setLoaddingValue(0.5f + AO.progress / 3);
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        _wndLoadding.setLoaddingValue(1.0f);
        yield return new WaitForSeconds(0.5f);
        Destroy(_wndLoadding.gameObject);
        sceneState = state;
    }
}
