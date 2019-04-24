using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveCardCoolTime : CoolTime {
    public uint cancelCooltimeCost;
    public MonoBehaviour behaviour;
    public GameObject targetCard;

    public override void Work() {
        GameObject deactive = targetCard.transform.Find("Deactive").gameObject;
        deactive.SetActive(true);
        Image image = deactive.GetComponent<Image>();
        image.fillAmount = 1 - currTime / coolTime;
    }

    public override void OnTime() {
        base.OnTime();
        GameObject deactive = targetCard.transform.Find("Deactive").gameObject;
        Image image = deactive.GetComponent<Image>();
        image.fillAmount = 1;
        deactive.SetActive(false);

        Destroy(GetComponent<ActiveCardCoolTime>());
    }
}
