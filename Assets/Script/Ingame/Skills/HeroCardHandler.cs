using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeroCardHandler : MonoBehaviour {
    ToggleGroup toggleGroup;
    public GameObject instantiatedUnitObj;
    public List<Vector3> path;

    void Awake() {
        GetComponent<Toggle>().onValueChanged.AddListener(delegate {
            ToggleValueChanged(GetComponent<Toggle>());
        });
        toggleGroup = transform.parent.GetComponent<ToggleGroup>();
    }

    void ToggleValueChanged(Toggle toggle) {
        if (toggle.isOn) {
            //Debug.Log(transform.Find("Name").GetComponent<Text>().text + "활성화됨");
            transform.Find("Selected").gameObject.SetActive(true);

            Cost cost = GetComponent<ActiveCardInfo>().data.baseSpec.unit.cost;
            PlayerController.Instance.GoldResourceFlick.GetComponent<Image>().fillAmount = (float)(cost.gold / 10);
            //Debug.Log(cost.population);
            //PlayerController.Instance.CitizenResourceFlick.GetComponent<Image>().fillAmount = (float)(cost.population / 10);
        }
        else {
            transform.Find("Selected").gameObject.SetActive(false);
        }

        PlayerController.Instance
            .HeroSummonListener()
            .ToggleListener(
                toggleGroup.AnyTogglesOn()
            );
    }
}
