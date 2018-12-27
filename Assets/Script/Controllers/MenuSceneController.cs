using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MenuSceneController : MonoBehaviour {

    public enum Windows {
        BASIC = 0,
        DECKLIST = 1,
        SHOP = 2
    }

    [SerializeField] GameObject[] windowObject = new GameObject[3];
    [SerializeField] Button[] buttonObject = new Button[3];

    private Windows openedWindow;

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.MenuScene;

    private void Awake() {
        
    }
    // Use this for initialization
    void Start() {
        openedWindow = Windows.BASIC;
        //buttonObject[0].onClick.AsObservable().Subscribe(_ => {
        //    buttonObject[0].transform.SetSiblingIndex(1);
        //    OpenWindow(0);
        //});
        //buttonObject[1].onClick.AsObservable().Subscribe(_ => {
        //    buttonObject[1].transform.SetSiblingIndex(1);
        //    OpenWindow(1);
        //});
        //buttonObject[2].onClick.AsObservable().Subscribe(_ => {
        //    buttonObject[2].transform.SetSiblingIndex(1);
        //    OpenWindow(2);
        //});
    }

    public void OpenWindow(int num) {
        buttonObject[num].transform.SetSiblingIndex(1);
        windowObject[(int)openedWindow].SetActive(false);
        windowObject[num].SetActive(true);
        openedWindow = (Windows)num;
    }

}
