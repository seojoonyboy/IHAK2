using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameHandChangeCoolTime : CoolTime {
    public GameObject Btn;

    public override void Work() {
        GameObject deactive = Btn.transform.Find("Deactive").gameObject;
        deactive.SetActive(true);
        Image image = deactive.GetComponent<Image>();
        image.fillAmount = 1 - currTime / coolTime;
    }

    public override void OnTime() {
        base.OnTime();

        GameObject deactive = Btn.transform.Find("Deactive").gameObject;
        Image image = deactive.GetComponent<Image>();
        image.fillAmount = 1;
        deactive.SetActive(false);

        Destroy(GetComponent<IngameHandChangeCoolTime>());
    }
}
