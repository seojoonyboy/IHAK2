using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingWindow : MonoBehaviour {

    [SerializeField] Image logoBar;
    [SerializeField] Image tipImage;
    [SerializeField] Text tipTxt;
    string[] tipStr = { "적당한 과금은 정신건강에 좋습니다.",
                        "덱 편집시 카드를 터치하면 정보를 볼 수 있습니다.",
                        "원시 종족 주민들은 빈손으로\n지구를 떠나 이 행성에 정착하였습니다.",
                        "환경을 관리하지 않으면\n유혈사태가 발생할 수(?)도 있습니다.", };

    public void initLoaddingLogo() {
        logoBar.fillAmount = 0.0f;
        int tip = Random.Range(0, tipStr.Length);
        tipTxt.text = tipStr[tip];
    }

    public void setLoaddingValue(float val) {
        logoBar.fillAmount = val;
    }
}
