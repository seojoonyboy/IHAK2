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
        toggleGroup = transform.parent.parent.GetComponent<ToggleGroup>();

        GetComponent<Toggle>().onValueChanged.AddListener(delegate {
            ToggleValueChanged(GetComponent<Toggle>());
        });
    }

    protected override void OnSingleClick() {
        if(instantiatedUnitObj != null) {
            IngameAlarm.instance.SetAlarm("영웅을 이동시킬 방향을 선택하세요.");

            UnitGroup unitGroup = instantiatedUnitObj.GetComponentInParent<UnitGroup>();
            if (unitGroup == null) return;

            unitGroup.checkWay();
        }
        else {
            Toggle toggle = GetComponent<Toggle>();
            toggle.isOn = !toggle.isOn;
            ToggleValueChanged(toggle);
        }
    }

    protected override void OnDoubleClick() {
        if (instantiatedUnitObj == null) return;
        MoveCamera();
    }

    void MoveCamera() {
        Vector3 pos = instantiatedUnitObj.transform.position;
        pos.z = Camera.main.transform.position.z;
        if (instantiatedUnitObj != null) {
            Camera.main.transform.position = pos;
        }
    }

    void ToggleValueChanged(Toggle toggle) {
        if (toggle.isOn) {
            transform.Find("Selected").gameObject.SetActive(true);

            Cost cost = GetComponent<ActiveCardInfo>().data.baseSpec.unit.cost;
            PlayerController.Instance.GoldResourceFlick.GetComponent<Image>().fillAmount = (float)(cost.gold / 10);

            PlayerController.Instance.myCity.Find("Portal").gameObject.SetActive(true);
        }
        else {
            transform.Find("Selected").gameObject.SetActive(false);

            PlayerController.Instance.myCity.Find("Portal").gameObject.SetActive(false);
        }

        PlayerController.Instance
            .HeroSummonListener()
            .ToggleListener(
                toggleGroup.AnyTogglesOn()
            );
    }
}
