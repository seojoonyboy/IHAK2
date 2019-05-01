using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;

public class PlayerPassiveCards : SerializedMonoBehaviour {
    public List<GameObject> passiveCards;
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<string, float> effectModules = new Dictionary<string, float>();

    [SerializeField] [ReadOnly] PlayerController playerController;
    [SerializeField] [ReadOnly] DeckInfo tileGroup;

    private IngameSceneEventHandler ingameSceneEventHandler;

    void Awake() {
        ingameSceneEventHandler = IngameSceneEventHandler.Instance;
        ingameSceneEventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, OnMyBuildingsAdded);
    }

    private void OnMyBuildingsAdded(Enum Event_Type, Component Sender, object Param) {
        Init();
    }

    public void Init() {
        playerController = PlayerController.Instance;
        var buildings = playerController.playerBuildings().buildingInfos;
        foreach(BuildingInfo buildingInfo in buildings) {
            /*
            if(buildingInfo.cardInfo.type == "passive") {
                passiveCards.Add(buildingInfo.gameObject);
                //AddEffectModule(buildingInfo.cardInfo.passiveSkills[0].method.methodName);
                TmpAddEffectModule(buildingInfo.cardInfo.id);
            }
            */
        }
        playerController.SetPassiveUI();
    }

    private void TmpAddEffectModule(string id) {
        switch (id) {
            //원시광장 (유닛 체력 증가)
            case "n_pm_02001":
                if (effectModules.ContainsKey("Unit_health")) {
                    effectModules["Unit_health"] += 10;
                }
                else {
                    effectModules.Add("Unit_health", 10);
                }
                break;
            //자연의 청소부 (유닛 사망시 골드 획득)
            case "n_pm_02004":
                if (effectModules.ContainsKey("Minion_die_gold")) {
                    effectModules["Minion_die_gold"] += 0.2f;
                }
                else {
                    effectModules.Add("Minion_die_gold", 0.2f);
                }
                break;
        }
    }

    private void AddEffectModule(string methodName) {

    }

    public enum Passives {
        mysterious_mine,
        primal_square,
        natural_scanvenger
    }
}
