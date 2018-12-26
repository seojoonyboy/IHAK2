using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingWindow : MonoBehaviour {

    [SerializeField] Slider _logo;

    public void initLoaddingLogo() {
        _logo.value = 0.0f;
    }

    public void setLoaddingValue(float val) {
        _logo.value = val;
    }
}
