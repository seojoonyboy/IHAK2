using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveCardCoolTime : CoolTime {
    public List<GameObject> cards;

    public GameObject card;
    public uint cancelCooltimeCost;

    public override void Work() {
        card = cards.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == gameObject);

        if (card == null) {
            cards.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == gameObject);

            if(cards == null) {
                Destroy(GetComponent<ActiveCardCoolTime>());
                return;
            }
        }

        ActiveCardInfo cardInfo = card.GetComponent<ActiveCardInfo>();
        int tier = 0;
        int lv = 0;

        if (!string.IsNullOrEmpty(cardInfo.data.baseSpec.unit.name)) {
            tier = 0;
            lv = cardInfo.data.ev.lv;
        }
        else if (!string.IsNullOrEmpty(cardInfo.data.baseSpec.skill.name)) {
            tier = cardInfo.data.baseSpec.skill.tierNeed;
            lv = 1;
        }
        cancelCooltimeCost = (uint)(Math.Round(
            (100.0f * (tier * lv / 25.0f)) * (coolTime - currTime),
            0,
            MidpointRounding.AwayFromZero));

        GameObject deactive = card.transform.Find("Deactive").gameObject;
        deactive.SetActive(true);
        Image image = deactive.GetComponent<Image>();
        image.fillAmount = 1 - currTime / coolTime;

        deactive.transform.Find("Button/Val").GetComponent<Text>().text = "비용 : " + cancelCooltimeCost;
    }

    public override void OnTime() {
        base.OnTime();
        if (card == null) {
            Destroy(GetComponent<ActiveCardCoolTime>());
            return;
        }
        GameObject deactive = card.transform.Find("Deactive").gameObject;
        Image image = deactive.GetComponent<Image>();
        image.fillAmount = 1;
        deactive.SetActive(false);

        Destroy(GetComponent<ActiveCardCoolTime>());
    }
}
