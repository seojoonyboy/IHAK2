using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionList : MonoBehaviour {

    public Sprite unclearSprite;
    public Sprite clearSprite;

    public Transform missionPivot;
    public Transform missionBtnList;
    public Slider missionSlider;
    public int selectMissionNum = -1;
    public AccountManager accountManager;
	// Use this for initialization
	void Start () {
        accountManager = AccountManager.Instance;
        missionBtnList = transform.Find("MissionList");
        missionPivot = transform.Find("location");
        missionSlider = transform.Find("missionSlider").GetComponent<Slider>();
        CheckClear();
    }

    public void CheckClear() {
        for (int i = 0; i < accountManager.missionClear + 1; i++) {
            missionBtnList.GetChild(i).GetComponent<Image>().sprite = clearSprite;
            missionPivot.position = new Vector3(missionBtnList.GetChild(i).position.x, 50);
            if (i == missionBtnList.childCount - 1) break;
        }
        missionSlider.value = (float)accountManager.missionClear / (missionBtnList.childCount - 1);
    }
	
    

    public void SelectMission(int num) {
        if (accountManager.missionClear < num) return;
        selectMissionNum = num;
        missionPivot.position = new Vector3(missionBtnList.GetChild(num).position.x, 50);
    }

    public void StartMission() {
        if (selectMissionNum < accountManager.missionClear) return;
    }

}
