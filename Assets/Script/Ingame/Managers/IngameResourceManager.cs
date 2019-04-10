using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameResourceManager : MonoBehaviour {

    [Header(" - ResourceUI")]
    [SerializeField] Text goldValue;
    [SerializeField] Image goldBar;
    [SerializeField] Text citizenValue;

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
            int _goldIncreaseAmount = 1;
            int _citizenIncreaseAmount = 1;

            yield return new WaitForSeconds(0.2f);

            if (gold >= 100) { _goldIncreaseAmount = 0; }
            if (citizen  >= 100) { _citizenIncreaseAmount = 0; }
            //if(gold == 299) { increaseAmnt = 1; }
            //goldBar.fillAmount = ((float)gold / 100);
            PlayerController.Instance.playerResource().Gold += _goldIncreaseAmount;
            PlayerController.Instance.playerResource().Citizen += _citizenIncreaseAmount;
            //goldValue.text = ((int)(gold / 10)).ToString();
        }
    }

    public void RefreshGoldSlider() {
        float gold = (float)PlayerController.Instance.playerResource().Gold;
        goldBar.fillAmount = ((float)gold / 100);
        goldValue.text = ((int)(gold / 10)).ToString();
    }

    public void RefreshCitizenText() {
        float citizen = (float)PlayerController.Instance.playerResource().Citizen;
        citizenValue.text = ((int)(citizen / 10)).ToString();
    }
}
