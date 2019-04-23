using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionList : MonoBehaviour {

    public Sprite unclearSprite;
    public Sprite clearSprite;
    
    public Transform missionBtnList;
    public int selectMissionNum = -1;
    public AccountManager accountManager;
	// Use this for initialization
	void Start () {
        accountManager = AccountManager.Instance;
        missionBtnList = transform.Find("MissionList");
        gameObject.SetActive(false);
        CheckClear();
    }

    public void CheckClear() {
        for(int i = 0; i< accountManager.missionClear; i++) 
            missionBtnList.GetChild(i).GetComponent<Image>().sprite = clearSprite;        
    }
	

    public void SelectMission(int num) {
        if (accountManager.selectNumber < num) return;
        selectMissionNum = num;
    }

    public void StartMission() {
        if (selectMissionNum < accountManager.missionClear) return;
    }

    public void CloseWindow() {
        gameObject.SetActive(false);
    }

}
