using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Container {
    public class PlayerActiveCards : MonoBehaviour {
        public List<ActiveCard> activeCards;
        [SerializeField] [ReadOnly] TileGroup tileGroup;
        PlayerController playerController;

        void Start() {
            playerController = GetComponent<PlayerController>();
            try {
                tileGroup =
                playerController.maps[PlayerController.Player.PLAYER_1]
                .transform.GetChild(0)
                .gameObject
                .GetComponent<TileGroup>();

                Init();
            }
            catch(NullReferenceException ex) {
                Debug.LogError("TileGroup을 찾을 수 없습니다.");
            }
        }

        public void Init() {
            activeCards = new List<ActiveCard>();

            var units = tileGroup.units;
            var spells = tileGroup.spells;
            foreach(ActiveCard unitCard in units) {
                unitCard.baseSpec.skill = null;
            }
            foreach(ActiveCard spellCard in spells) {
                spellCard.baseSpec.unit = null;
            }
            activeCards.AddRange(units);
            activeCards.AddRange(spells);
        }
    }
}

[System.Serializable]
public class ActiveCard {
    public GameObject parentBuilding;
    public GameObject gameObject;
    public BaseSpec baseSpec = new BaseSpec();
    public Ev ev;
}