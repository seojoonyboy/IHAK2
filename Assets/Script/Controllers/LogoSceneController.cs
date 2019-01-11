using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogoSceneController : MonoBehaviour {

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.LogoScene;

    public void pressAnyKey() {
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        transform.Find("Logo").gameObject.SetActive(false);
        NetworkManager.Instance.request(deviceID, NetworkManager.Instance.baseUrl + "/api/users/deviceid/{deviceId}", NetworkManager.Callback);
        
        
    }

    public void startButton() {
        GameSceneManager mvc = FindObjectOfType<GameSceneManager>();
        mvc.startScene(sceneState, GameSceneManager.SceneState.MenuScene);
    }
}
