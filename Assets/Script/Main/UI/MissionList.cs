using System;
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
    public Text missionText;

    public int selectMissionNum = -1;
    public AccountManager accountManager;
    public GameObject startButton;
    public GameObject unlockMissionBtn;
	// Use this for initialization
	void Start () {
        accountManager = AccountManager.Instance;
        missionBtnList = transform.Find("MissionList");
        missionPivot = transform.Find("location");
        missionSlider = transform.Find("missionSlider").GetComponent<Slider>();
        missionText = transform.Find("missionText").GetComponent<Text>();
        CheckClear();
        
    }

    

    public void CheckClear() {
        for (int i = 0; i < accountManager.missionClear + 1; i++) {
            missionBtnList.GetChild(i).GetComponent<Image>().sprite = clearSprite;
            missionPivot.position = new Vector3(missionBtnList.GetChild(i).position.x, 50);
            if (i == missionBtnList.childCount - 1) break;
        }
        missionSlider.value = (float)accountManager.missionClear / (missionBtnList.childCount - 1);

        if (accountManager.missionClear >= 3)
            SelectMission(2);
        else if (accountManager.missionClear <= 0)
            SelectMission(0);
        else
            SelectMission(accountManager.missionClear);
    }
	
    

    public void SelectMission(int num) {
        selectMissionNum = num;
        switch (selectMissionNum) {
            case 0:
                missionPivot.position = new Vector3(missionBtnList.GetChild(selectMissionNum).position.x, 50);
                missionText.text = "미션 1 : 이동, 점령, 마법";
                break;
            case 1:
                missionPivot.position = new Vector3(missionBtnList.GetChild(selectMissionNum).position.x, 50);
                missionText.text = "미션 2 : 거점종류, 레벨업, 후퇴";
                break;
            case 2:
                missionPivot.position = new Vector3(missionBtnList.GetChild(selectMissionNum).position.x, 50);
                missionText.text = "미션 3 : 더미도시와 겨루기";
                break;
            default:
                missionPivot.position = new Vector3(missionBtnList.GetChild(2).position.x, 50);
                missionText.text = "미션 3 : 더미도시와 겨루기";
                break;
        }

        if (accountManager.missionClear < num) {
            startButton.GetComponent<Image>().color = Color.gray;
            startButton.transform.Find("Lock").gameObject.SetActive(true);
        }
        else {
            startButton.GetComponent<Image>().color = Color.white;
            startButton.transform.Find("Lock").gameObject.SetActive(false);
        }

    }
    


    public void StartMission() {
        if (selectMissionNum > accountManager.missionClear) return;
        accountManager.RequestMissionDeck(selectMissionNum + 1);
    }

    public void unlockMission() {
        if(accountManager.missionClear < 2) {
            accountManager.missionClear = 3;

            for (int i = 0; i < transform.Find("MissionList").childCount; i++) {
                missionBtnList.GetChild(i).GetComponent<Image>().sprite = clearSprite;                
            }
            missionPivot.position = new Vector3(missionBtnList.GetChild(2).position.x, 50);
            missionSlider.value = (float)accountManager.missionClear / (missionBtnList.childCount - 1);
            missionText.text = "미션 3 : 더미도시와 겨루기";

            startButton.GetComponent<Image>().color = Color.white;
            startButton.transform.Find("Lock").gameObject.SetActive(false);
        }
    }



}
