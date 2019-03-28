using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameResourceManager : MonoBehaviour {

    [Header(" - ResourceUI")]
    [SerializeField] Text goldValue;
    [SerializeField] Image goldBar;

    private IEnumerator goldProducer;
    private IEnumerator envProducer;

    private void Awake() {
        goldProducer = GoldProduce();
        envProducer = EnvProduce();
    }

    public void OnGoldProduce(bool start) {
        if(start)
            StartCoroutine(goldProducer);
        else
            StopCoroutine(goldProducer);
    }

    private IEnumerator GoldProduce() {
        while (true) {
            RefreshGoldSlider();
            float gold = (float)PlayerController.Instance.playerResource().Gold;
            int increaseAmnt = 2;
            yield return new WaitForSeconds(1.0f);

            if (gold >= 30) { increaseAmnt = 0; }
            if(gold == 29) { increaseAmnt = 1; }

            goldBar.fillAmount = ((float)gold / 30);
            PlayerController.Instance.playerResource().Gold += increaseAmnt;
            goldValue.text = gold.ToString();
        }
    }

    public void RefreshGoldSlider() {
        float gold = (float)PlayerController.Instance.playerResource().Gold;
        goldBar.fillAmount = ((float)gold / 30);
        goldValue.text = gold.ToString();
    }

    public void OnEnvProduce(bool start) {
        
    }

    private IEnumerator EnvProduce() {
        while (true) {
            float env = (float)PlayerController.Instance.playerResource().Env;
            yield return new WaitForSeconds(1.0f);
        }
    }
}
