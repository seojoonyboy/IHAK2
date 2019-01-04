using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour {
    public enum SceneState {
        None,
        LogoScene,
        MenuScene,
        DeckSettingScene,
        InGameScene,
    }

    private SceneState sceneState;

    PlayerInfosManager pim;

    [SerializeField] GameObject loaddingWnd;

    private void Awake() {
        sceneState = SceneState.None;
        pim = PlayerInfosManager.Instance;
    }

    // Use this for initialization
    void Start() {
        startScene(SceneState.None, SceneState.LogoScene);
    }

    public void startScene(SceneState thisScene, SceneState targetScene) {
        StartCoroutine(LoaddingScene(thisScene, targetScene.ToString(), targetScene));
    }

    IEnumerator LoaddingScene(SceneState remove, string load, SceneState state) {
        sceneState = SceneState.None;
        AsyncOperation AO;
        GameObject go = Instantiate(loaddingWnd);
        LoadingWindow _wndLoadding;
        _wndLoadding = go.GetComponent<LoadingWindow>();
        _wndLoadding.initLoaddingLogo();

        if (remove != SceneState.None) {
            AO = SceneManager.UnloadSceneAsync(remove.ToString());
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
