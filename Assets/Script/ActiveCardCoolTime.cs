using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveCardCoolTime : CoolTime {
    public List<GameObject> 
        Hand,
        Deck;

    public GameObject card;
    public override void Work() {
        card = Hand.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == gameObject);

        if (card == null) {
            Deck.Find(x => x.GetComponent<ActiveCardInfo>().data.parentBuilding == gameObject);

            if(Deck == null) {
                Destroy(GetComponent<ActiveCardCoolTime>());
                return;
            }
        }

        GameObject deactive = card.transform.Find("Deactive").gameObject;
        deactive.SetActive(true);
        Image image = deactive.GetComponent<Image>();
        image.fillAmount = 1 - currTime / coolTime;
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
