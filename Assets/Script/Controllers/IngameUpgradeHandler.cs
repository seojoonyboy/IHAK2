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

    int index = 0;      //Gage의 Index

    public int Index {
        get { return index; }
        set { index = value; }
    }

    public void Init(List<Magnification> list) {
        string id = GetComponent<StringIndex>().Id;
        Magnification result = null;
        foreach(Magnification magnification in list) {
            if (magnification.key.Contains(id)) result = magnification;
        }
        if (result == null) return;
        value.text = "x" + string.Format(CultureInfo.InvariantCulture, "0.00" , result.current_mag);
    }
}
