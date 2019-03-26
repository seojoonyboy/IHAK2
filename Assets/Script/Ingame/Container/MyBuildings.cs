using System;
using System.Collections.Generic;
using UnityEngine;
using DataModules;

namespace Container {
    public class MyBuildings : PlayerBuildings {
        private IngameSceneEventHandler ingameSceneEventHandler;

        public int[] demoTileIndex;

        private void Awake() {
            ingameSceneEventHandler = IngameSceneEventHandler.Instance;
        }

        public override void Init() {
            playerController = PlayerController.Instance;
            try {
                tileGroup =
                playerController.maps[PlayerController.Player.PLAYER_1]
                .transform.GetChild(0)
                .gameObject
                .GetComponent<TileGroup>();
            }
            catch (NullReferenceException ex) {
                Debug.LogError("TileGroup을 찾을 수 없습니다.");
                return;
            }

            for (int i = 0; i < demoTileIndex.Length; i++) {    // 3x3 마을용 연산
                BuildingInfo bi = new BuildingInfo();
                bi.tileNum = demoTileIndex[i];
                bi.activate = true;
                GameObject buildingGo = tileGroup
                    .transform
                    .GetChild(demoTileIndex[i])
                    .GetChild(0)
                    .gameObject;

                bi.gameObject = buildingGo;
                bi.cardInfo = buildingGo.GetComponent<BuildingObject>().card.data;
                bi.hp = bi.maxHp = bi.cardInfo.hitPoint;
                buildingInfos.Add(bi);
            }

            ingameSceneEventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, this, buildingInfos);
        }
    }
}
