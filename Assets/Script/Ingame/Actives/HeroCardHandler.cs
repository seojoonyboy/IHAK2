using DataModules;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeroCardHandler : IngameCardHandler {
    ToggleGroup toggleGroup;
    public GameObject instantiatedUnitObj;
    public List<Vector3> path;

    void Awake() {
        toggleGroup = transform.parent.GetComponent<ToggleGroup>();
        GetComponent<Toggle>().onValueChanged.AddListener(delegate {
            ToggleValueChanged(GetComponent<Toggle>());
        });
    }

    protected override void OnSingleClick() {
        Toggle toggle = GetComponent<Toggle>();
        toggle.isOn = !toggle.isOn;
        ToggleValueChanged(toggle);
    }

    protected override void OnDoubleClick() {
        Vector3 pos = instantiatedUnitObj.transform.position;
        pos.z = -10f;
        if(instantiatedUnitObj != null) {
            Camera.main.transform.position = pos;
        }
    }

    void ToggleValueChanged(Toggle toggle) {
        if (toggle.isOn) {
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
