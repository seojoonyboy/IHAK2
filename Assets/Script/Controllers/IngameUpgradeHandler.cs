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
    [SerializeField] IngameUpgradeStream stream;

    int myMagnification_index;      //Magnification List에서 내 Magnification에 대한 Index
    int index = 0;                  //Gage의 Index
    string id;
    Magnification stream_magnification;

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
        stream_magnification = new Magnification(ingameCityManager.myBuildings_mags[myMagnification_index].key, ingameCityManager.myBuildings_mags[myMagnification_index].mag, ingameCityManager.myBuildings_mags[myMagnification_index].max_mag, ingameCityManager.myBuildings_mags[myMagnification_index].max_point);
        stream_magnification.current_point = ingameCityManager.myBuildings_mags[myMagnification_index].current_point;

        Index = stream_magnification.current_point;

        for(int i=0; i<Index; i++) {
            slider.GetChild(i).gameObject.SetActive(true);
        }

        for(int i=Index; i<slider.childCount; i++) {
            slider.GetChild(i).gameObject.SetActive(false);
        }
        //test
        //playerController.Point += 10;
    }

    public void OnAddBtnClick() {
        if (canAdd()) {
            slider.GetChild(Index).gameObject.SetActive(true);
            Index++;

            stream.Add(id);
            stream_magnification.current_point++;
            stream_magnification.current_mag += stream_magnification.mag;

            value.text = "x " + stream_magnification.current_mag;
        }
        else {
            Debug.Log("포인트 부족/초과");
        }
    }

    public void OnRemoveBtnClick() {
        //이전에 할당된 포인트는 제거할 수 없음
        if (canRemove()) {
            Index--;
            slider.GetChild(Index).gameObject.SetActive(false);

            stream.Remove(id);
            stream_magnification.current_point--;
            stream_magnification.current_mag -= stream_magnification.mag;

            value.text = "x " + stream_magnification.current_mag;
        }
        else {
            Debug.Log("더이상 수정 불가");
        }
    }

    private bool canAdd() {
        if (stream.Point <= 0) return false;
        if (stream_magnification.current_point >= stream_magnification.max_point) return false;
        return true;
    }

    private bool canRemove() {
        if (ingameCityManager.myBuildings_mags[myMagnification_index].current_point >= Index) return false;
        return true;
    }
}
