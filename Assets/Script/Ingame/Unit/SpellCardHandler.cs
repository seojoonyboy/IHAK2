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
    public GameObject targetCard;

    private GameObject instantiatedPrefab;
    Toggle toggle;

    void Awake() {
        toggleGroup = transform.parent.parent.GetComponent<ToggleGroup>();
        toggle = GetComponent<Toggle>();
    }

    protected override void OnSingleClick() {
        if (GetComponent<ActiveCardCoolTime>() != null) return;

        toggle.isOn = !toggle.isOn;
        if (toggle.isOn) {
            instantiatedPrefab = Instantiate(prefab, PlayerController.Instance.spellPrefabParent);
            instantiatedPrefab.GetComponent<SpellCardDragHandler>().Init(
                camera: Camera.main,
                parentBuilding: parentBuilding,
                deckShuffler: PlayerController.Instance.deckShuffler(),
                data: data,
                coolTime: coolTime,
                targetCard: targetCard
            );

            Vector3 camPos = Camera.main.transform.position;
            camPos.z = 0;
            instantiatedPrefab.transform.position = camPos;

            transform.Find("Selected").gameObject.SetActive(true);
        }
        else {
            Cancel();
        }
    }

    //마법을 사용하지 않고 취소
    public void Cancel() {
        toggle.isOn = false;
        transform.Find("Selected").gameObject.SetActive(false);
        Destroy(instantiatedPrefab);
    }

    public void Handle() {
        toggle.isOn = false;
        transform.Find("Selected").gameObject.SetActive(false);
    }
}
