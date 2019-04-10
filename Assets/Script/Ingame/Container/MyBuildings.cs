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

            for (int i = 0; i < tileGroup.transform.GetChild(0).childCount; i++) {    // 3x3 마을용 연산
                BuildingInfo bi = new BuildingInfo();
                bi.tileNum = demoTileIndex[i];
                bi.activate = true;
                try {
                    GameObject buildingGo = tileGroup
                    .transform
                    .GetChild(demoTileIndex[i])
                    .GetChild(0)
                    .gameObject;

                    bi.gameObject = buildingGo;
                    bi.cardInfo = buildingGo.GetComponent<BuildingObject>().card.data;
                    buildingInfos.Add(bi);
                }
                catch(NullReferenceException ex) { }
            }

            ingameSceneEventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, this, buildingInfos);
        }

        public void RemoveTile() {
            for(int i = 0 ; i < tileGroup.transform.childCount - 1 ; i++) {
                tileGroup
                    .transform
                    .GetChild(i)
                    .GetComponent<SpriteRenderer>()
                    .enabled = false;
            }
        }
    }
}
