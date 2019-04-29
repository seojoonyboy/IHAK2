using Container;
using DataModules;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBuildings : PlayerBuildings {

    public override void Init() {
        //playerController = PlayerController.Instance;

        //try {
        //    tileGroup =
        //    playerController.maps[PlayerController.Player.PLAYER_2]
        //    .transform.GetChild(0)
        //    .gameObject
        //    .GetComponent<DeckInfo>();
        //}
        //catch (NullReferenceException ex) {
        //    Debug.LogError("TileGroup을 찾을 수 없습니다.");
        //    return;
        //}

        //tileGroup.transform.Find("Background").gameObject.SetActive(false);
        
        //foreach (Transform tile in tileGroup.gameObject.transform) {
        //    if (tile.childCount == 1) {
        //        int tileNum = tile.GetComponent<TileObject>().tileNum;
        //        GameObject building = tile.GetChild(0).gameObject;
        //        BuildingObject buildingObject = building.GetComponent<BuildingObject>();
        //        CardData card = buildingObject.card.data;

        //        BuildingInfo info = new BuildingInfo(
        //            tileNum: tileNum,
        //            activate: true,
        //            card: card,
        //            gameObject: building
        //        );
        //        buildingInfos.Add(info);
        //    }
        //}
        GetComponent<PlayerResource>().maxhp = GetComponent<PlayerResource>().TotalHp = 1000;
        IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.EVENT_TYPE.ENEMY_BUILDINGS_INFO_ADDED, this, buildingInfos);
    }
}
