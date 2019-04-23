using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeroCardHandler : MonoBehaviour, IPointerClickHandler {
    void Awake() {
        GetComponent<Toggle>().onValueChanged.AddListener(delegate {
            ToggleValueChanged(GetComponent<Toggle>());
        });
    }

    void ToggleValueChanged(Toggle toggle) {
        if (toggle.isOn) {
            Debug.Log(transform.Find("Name").GetComponent<Text>().text + "활성화됨");
            transform.Find("Selected").gameObject.SetActive(true);
        }
        else {
            transform.Find("Selected").gameObject.SetActive(false);
        }
    }
    public void OnPointerClick(PointerEventData eventData) {
        PlayerController.Instance
            .HeroSummonListener()
            .ToggleListener(
                !(transform.parent.GetComponent<ToggleGroup>()
                .AnyTogglesOn())
            );
    }
}
