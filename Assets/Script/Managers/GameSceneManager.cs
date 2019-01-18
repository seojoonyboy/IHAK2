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
        IngameScene,
    }

    private SceneState sceneState;

    AccountManager accountManager;

    [SerializeField] GameObject loaddingWnd;

    private void Awake() {
        sceneState = SceneState.None;
        accountManager = AccountManager.Instance;
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
        if (state == SceneState.MenuScene)
            MenuSceneEventHandler.Instance.PostNotification(MenuSceneEventHandler.EVENT_TYPE.SET_TILE_OBJECTS_COMPLETED, null, AccountManager.Instance.leaderIndex);
        Destroy(_wndLoadding.gameObject);
        sceneState = state;
    }
}
