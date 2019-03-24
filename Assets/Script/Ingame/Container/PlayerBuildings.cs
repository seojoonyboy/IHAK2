using DataModules;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Container {
    public class PlayerBuildings : MonoBehaviour {
        [SerializeField] [ReadOnly] TileGroup tileGroup;
        [SerializeField] [ReadOnly] PlayerController playerController;
        private IngameSceneEventHandler ingameSceneEventHandler;

        public int[] demoTileIndex;
        public List<BuildingInfo> myBuildingsInfo = new List<BuildingInfo>();

        private void Awake() {
            ingameSceneEventHandler = IngameSceneEventHandler.Instance;
        }

        public void Init() {
            playerController = GetComponent<PlayerController>();
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
                //cityHP += bi.hp;
                myBuildingsInfo.Add(bi);
            }

            ingameSceneEventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, this, myBuildingsInfo);
        }
    }

    [Serializable]
    public class BuildingInfo {
        public int tileNum;
        public bool activate;
        public int hp;
        public int maxHp;
        public CardData cardInfo;
        public GameObject gameObject;
        public BuildingInfo() { }

        public BuildingInfo(int tileNum, bool activate, int hp, int maxHp, CardData card, GameObject gameObject) {
            this.tileNum = tileNum;
            this.activate = activate;
            this.hp = hp;
            this.maxHp = maxHp;
            this.cardInfo = card;
            this.gameObject = gameObject;
        }
    }
}
