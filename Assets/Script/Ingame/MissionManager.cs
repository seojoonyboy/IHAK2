using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Container;
using Sirenix.OdinInspector;
using TMPro;
using System.Text;
using ingameUIModules;

public class MissionManager : SerializedMonoBehaviour {

    IngameSceneEventHandler eventHandler;
    public int stageNum;
    public int count = 0;
    //public bool[] clear;
    
    public StageGoal stageGoals;


    public class StageGoal {
        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public Dictionary<string, string> missionLists;
    }

    private void Start() {
        stageNum = AccountManager.Instance.mission.stageNum;
        eventHandler = IngameSceneEventHandler.Instance;
        SetMissionListener();
    }

    private void OnDestroy() {
        DestroyMissionListener();
    }

    private void SetMissionListener() {
        eventHandler.AddListener(IngameSceneEventHandler.MISSION_EVENT.MOVE_COMPLETE, UnitMoveComplete);
        eventHandler.AddListener(IngameSceneEventHandler.MISSION_EVENT.NODE_CAPTURE_COMPLETE, PointCaptured);
        eventHandler.AddListener(IngameSceneEventHandler.MISSION_EVENT.USE_MAGIC, UseMagic);
        eventHandler.AddListener(IngameSceneEventHandler.MISSION_EVENT.DESTROYED_ENEMY_CITY, DestroyEnemy);
        eventHandler.AddListener(IngameSceneEventHandler.MISSION_EVENT.UNIT_LEVEL_UP, LevelUP);
    }

    private void UnitMoveComplete(Enum Event_Type, Component Sender, object Param = null) {
        if (stageNum == 1) eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, null, "1-1");
        else if (stageNum == 2) eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, this, "2-1");
    }

    private void PointCaptured(Enum Event_Type, Component Sender, object Param) {
        if (stageNum == 1) {
            eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, this, "1-2");
        }
        else if (stageNum == 2 && count < 2) {
            count++;
            if (count >= 2)
                eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, this, "2-3");

        }
    }

    private void UseMagic(Enum Event_Type, Component Sender, object Param) {
        Debug.Log(Event_Type + " , " + Sender + " , " + Param);
        if (stageNum == 1) {
            eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, this, "1-3");
        }
    }

    private void DestroyEnemy(Enum Event_Type, Component Sender, object Param) {
        if (stageNum == 1) eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, this, "1-4");
        else if (stageNum == 2) eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, this, "2-4");
        if (stageNum == 3) eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, this, "3-1");
    }

    private void LevelUP(Enum Event_Type, Component Sender, object Param) {
        if (stageNum == 2)
            eventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.SUB_MISSION_COMPLETE, this, "2-2");
    }


    private void DestroyMissionListener() {
        eventHandler.RemoveListener(IngameSceneEventHandler.MISSION_EVENT.MOVE_COMPLETE, UnitMoveComplete);
        eventHandler.RemoveListener(IngameSceneEventHandler.MISSION_EVENT.NODE_CAPTURE_COMPLETE, PointCaptured);
        eventHandler.RemoveListener(IngameSceneEventHandler.MISSION_EVENT.USE_MAGIC, UseMagic);
        eventHandler.RemoveListener(IngameSceneEventHandler.MISSION_EVENT.DESTROYED_ENEMY_CITY, DestroyEnemy);
        eventHandler.RemoveListener(IngameSceneEventHandler.MISSION_EVENT.UNIT_LEVEL_UP, LevelUP);
    }
}
