using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameResourceManager : MonoBehaviour {

    [Header(" - ResourceUI")]
    [SerializeField] Text goldValue;
    [SerializeField] Image goldBar;

    private IEnumerator goldProducer;

    private void Awake() {
        goldProducer = GoldProduce();
    }

    public void OnGoldProduce(bool start) {
        if(start)
            StartCoroutine(goldProducer);
        else
            StopCoroutine(goldProducer);
    }

    private IEnumerator GoldProduce() {
        while (true) {
            float gold = (float)PlayerController.Instance.playerResource().Gold;
            yield return new WaitForSeconds(1.0f);
            gold += 2;
            if (gold > 30)
                gold = 30;
            goldBar.fillAmount = ((float)gold / 30);
            PlayerController.Instance.playerResource().Gold = (int)gold;
            goldValue.text = gold.ToString();
        }
    }
}