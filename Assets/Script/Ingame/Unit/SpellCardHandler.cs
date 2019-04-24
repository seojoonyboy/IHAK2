using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellCardHandler : IngameCardHandler {
    ToggleGroup toggleGroup;
    public GameObject prefab;
    public string[] data;
    public GameObject parentBuilding;
    public int coolTime;

    private GameObject instantiatedPrefab;

    void Awake() {
        toggleGroup = transform.parent.parent.GetComponent<ToggleGroup>();

        GetComponent<Toggle>().onValueChanged.AddListener(delegate {
            ToggleValueChanged(GetComponent<Toggle>());
        });
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
            Destroy(instantiatedPrefab);
        }

        PlayerController.Instance
            .HeroSummonListener()
            .ToggleListener(
                toggleGroup.AnyTogglesOn()
            );
    }

    protected override void OnSingleClick() {
        Toggle toggle = GetComponent<Toggle>();
        toggle.isOn = !toggle.isOn;
        if (toggle.isOn) {
            instantiatedPrefab = Instantiate(prefab, PlayerController.Instance.spellPrefabParent);
            instantiatedPrefab.GetComponent<SpellCardDragHandler>().Init(
                camera: Camera.main,
                parentBuilding: parentBuilding,
                deckShuffler: PlayerController.Instance.deckShuffler(),
                data: data,
                coolTime: coolTime
            );

            Vector3 camPos = Camera.main.transform.position;
            camPos.z = 0;
            instantiatedPrefab.transform.position = camPos;
        }
        else {
            Destroy(instantiatedPrefab);
        }

        ToggleValueChanged(toggle);
    }
}