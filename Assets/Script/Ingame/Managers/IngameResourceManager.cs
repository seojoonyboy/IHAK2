using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameResourceManager : MonoBehaviour {

    [Header(" - ResourceUI")]
    [SerializeField] Text goldValue;
    [SerializeField] Image goldBar;
    [SerializeField] Text citizenValue;
    [SerializeField] Image citizenBar;

    private IEnumerator resourceProducer;

    private void Awake() {
        resourceProducer = ResourceProduce();
    }

    public void OnResourceProduce(bool start) {
        if(start)
            StartCoroutine(resourceProducer);
        else
            StopCoroutine(resourceProducer);
    }

    private IEnumerator ResourceProduce() {
        while (true) {
            //RefreshGoldSlider();
            float gold = (float)PlayerController.Instance.playerResource().Gold;
            float citizen = (float)PlayerController.Instance.playerResource().Citizen;
            int _goldIncreaseAmount = 10;
            int _citizenIncreaseAmount = 4;

            yield return new WaitForSeconds(0.5f);

            if (gold >= 1000) { _goldIncreaseAmount = 0; }
            if (citizen  >= 1000) { _citizenIncreaseAmount = 0; }
            //if(gold == 299) { increaseAmnt = 1; }
            //goldBar.fillAmount = ((float)gold / 100);
            PlayerController.Instance.playerResource().Gold += _goldIncreaseAmount;
            PlayerController.Instance.playerResource().Citizen += _citizenIncreaseAmount;
            //goldValue.text = ((int)(gold / 10)).ToString();
        }
    }

    public void RefreshGold() {
        float gold = (float)PlayerController.Instance.playerResource().Gold;
        goldBar.fillAmount = ((float)gold / 1000);
        goldValue.text = ((int)(gold / 100)).ToString();
    }

    public void RefreshCitizen() {
        float citizen = (float)PlayerController.Instance.playerResource().Citizen;
        citizenBar.fillAmount = ((float)citizen / 1000);
        citizenValue.text = ((int)(citizen / 100)).ToString();
    }
}
