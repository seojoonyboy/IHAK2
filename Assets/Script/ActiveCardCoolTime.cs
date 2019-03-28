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
        //ActiveCardInfo cardInfo = GetComponent<ActiveCardInfo>();
        //int tier = 0;
        //int lv = 0;

        //if (!string.IsNullOrEmpty(cardInfo.data.baseSpec.unit.name)) {
        //    tier = 0;
        //    lv = cardInfo.data.ev.lv;
        //}
        //else if (!string.IsNullOrEmpty(cardInfo.data.baseSpec.skill.name)) {
        //    tier = cardInfo.data.baseSpec.skill.tierNeed;
        //    lv = 1;
        //}
        //cancelCooltimeCost = (uint)(Math.Round(
        //    (100.0f * (tier * lv / 25.0f)) * (coolTime - currTime),
        //    0,
        //    MidpointRounding.AwayFromZero));

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

        behaviour.enabled = true;

        Destroy(GetComponent<ActiveCardCoolTime>());
    }
}
