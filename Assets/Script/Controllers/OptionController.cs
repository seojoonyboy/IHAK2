using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using UniRx;
using UniRx.Triggers;

public class OptionController : MonoBehaviour {

    [SerializeField] GameObject optionWindow;
    [SerializeField] Transform[] settingWindows;
    [SerializeField] GameObject[] switchList; //index => 0: 효과음 / 1: 배경음 / 2: 광고팝업 / 3: 야간팝업
    private bool opend = false;
    

    private void Start() {
        var esc = Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Escape));
        esc.Where(_ => !FindObjectOfType<LoadingWindow>()).Subscribe(_ => EnterOptionWindow(!opend));
    }

    public void SetWindow(int num) {  //num =  1 : 계정설정, 2 : 게임설정, 3 : 언어설정
        settingWindows[num].SetSiblingIndex(2);
    }

    public void EnterOptionWindow(bool enter) { // enter = true 일시 활성화
        if(AccountManager.Instance.scenestate > GameSceneManager.SceneState.LogoScene)
            optionWindow.SetActive(enter);
        if (enter) {
            settingWindows[0].SetSiblingIndex(2);
            for (int i = 0; i < switchList.Length; i++) {
                if (!switchList[i].GetComponent<SwitchButton>().switchOn)
                    switchList[i].GetComponent<Animator>().SetTrigger("IsOffWhenGameStart");
            }
        }
        opend = !opend;
    }
}
