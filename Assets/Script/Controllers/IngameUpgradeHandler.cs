using DataModules;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class IngameUpgradeHandler : MonoBehaviour {
    [SerializeField] Text value;
    [SerializeField] Transform slider;
    [SerializeField] Button decreaseBtn;
    [SerializeField] Button increaseBtn;
    [SerializeField] IngameCityManager ingameCityManager;
    [SerializeField] PlayerController playerController;

    int myMagnification_index;      //Magnification List에서 내 Magnification에 대한 Index
    int index = 0;                  //Gage의 Index
    string id;

    public int Index {
        get { return index; }
        set { index = value; }
    }

    public void Init(List<Magnification> list) {
        id = GetComponent<StringIndex>().Id;
        Magnification result = null;
        string str = "1.00";

        int count = 0;
        foreach(Magnification magnification in list) {
            if (magnification.key.Contains(id)) {
                result = magnification;
                myMagnification_index = count;
            }
            count++;
        }
        if (result != null) {
            str = string.Format("{0:0.00}", result.current_mag);
        }
        value.text = "x " + str;

        //test
        //playerController.Point += 10;
    }

    public void OnAddBtnClick() {
        if (canAdd()) {
            Magnification magnification = ingameCityManager.myBuildings_mags[myMagnification_index];
            magnification.current_point++;
            magnification.current_mag += magnification.mag;

            value.text = "x " + string.Format("{0:0.00}", magnification.current_mag);

            playerController.Point--;

            slider.GetChild(Index).gameObject.SetActive(true);
            Index++;

            playerController.GetComponent<IngameUpgradeStream>().Add(id);
        }
        else {
            Debug.Log("포인트 부족/초과");
        }
    }

    public void OnRemoveBtnClick() {
        //이전에 할당된 포인트는 제거할 수 없음

    }

    private bool canAdd() {
        if (playerController.Point <= 0) return false;
        Magnification magnification = ingameCityManager.myBuildings_mags[myMagnification_index];
        if (magnification.current_point >= magnification.max_point) return false;
        return true;
    }

    private bool canRemove() {

        return true;
    }
}
