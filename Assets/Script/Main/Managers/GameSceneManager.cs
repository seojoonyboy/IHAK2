using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour {
    public enum SceneState {
        None = 0,
        LogoScene = 1,
        MenuScene = 2,
        DeckSettingScene = 3,
        IngameScene = 4,
    }

    private SceneState sceneState;

    AccountManager accountManager;

    [SerializeField] GameObject loaddingWnd;

    private void Awake() {
        sceneState = SceneState.None;
        accountManager = AccountManager.Instance;
        Application.targetFrameRate = 600;
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
        if(isIngame(state)) {
            yield return SceneManager.LoadSceneAsync(
                string.Format("stage{0}", AccountManager.Instance.mission.stageNum), 
                LoadSceneMode.Additive);
        }
        yield return new WaitForSeconds(0.5f);
        _wndLoadding.setLoaddingValue(0.5f);
        yield return new WaitForSeconds(0.5f);
        
        AO = SceneManager.LoadSceneAsync(load, LoadSceneMode.Additive);
        while (!AO.isDone) {
            _wndLoadding.setLoaddingValue(0.5f + AO.progress / 3);
            yield return null; 
        }
        missionIngame(state);
        yield return new WaitForSeconds(0.5f);
        _wndLoadding.setLoaddingValue(1.0f);
        yield return new WaitForSeconds(0.5f);
        if (state == SceneState.MenuScene)
            MenuSceneEventHandler.Instance.PostNotification(MenuSceneEventHandler.EVENT_TYPE.INITIALIZE_DECK, null, AccountManager.Instance.leaderIndex);
        Destroy(_wndLoadding.gameObject);
        sceneState = state;
        AccountManager.Instance.scenestate = state;
    }

    private bool isIngame(SceneState state) {
        if(AccountManager.Instance.mission == null) return false;
        if(state == SceneState.IngameScene) return true;
        return false;
    }

    private void missionIngame(SceneState state) {
        if(AccountManager.Instance.mission == null) return;
        if(state != SceneState.IngameScene) return;
        Scene ingame = SceneManager.GetSceneByName("IngameScene");
		Scene stage = SceneManager.GetSceneByName(string.Format("stage{0}", AccountManager.Instance.mission.stageNum));
        SceneManager.MergeScenes(stage, ingame);
    }
}
