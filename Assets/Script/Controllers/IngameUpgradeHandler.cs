using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameUpgradeHandler : MonoBehaviour {
    [SerializeField] Text value;
    [SerializeField] Transform slider;
    [SerializeField] Button decreaseBtn;
    [SerializeField] Button increaseBtn;

    int index = 0;

    public int Index {
        get { return index; }
        set { index = value; }
    }
}
