using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingWindow : MonoBehaviour {

    [SerializeField] Image logoBar;
    [SerializeField] Image tipImage;
    [SerializeField] Sprite[] tips;

    public void initLoaddingLogo() {
        logoBar.fillAmount = 0.0f;
        int tip = Random.Range(0, tips.Length);
        tipImage.sprite = tips[tip];
    }

    public void setLoaddingValue(float val) {
        logoBar.fillAmount = val;
    }
}
