using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Container {
    public class PlayerActiveCards : MonoBehaviour {
        public List<ActiveCard> activeCards;
        [SerializeField] [ReadOnly] DeckInfo deckInfo;
        [SerializeField] [ReadOnly] PlayerController playerController;

        public void Init() {
            playerController = PlayerController.Instance;
            try {
                deckInfo =
                playerController.maps[PlayerController.Player.PLAYER_1]
                .transform.GetChild(0)
                .gameObject
                .GetComponent<DeckInfo>();
            }
            catch (NullReferenceException ex) {
                Debug.LogError("TileGroup을 찾을 수 없습니다.");
                return;
            }

            activeCards = new List<ActiveCard>();

            var units = deckInfo.units;
            var spells = deckInfo.spells;
            activeCards.AddRange(units);
            activeCards.AddRange(spells);
        }

        public List<ActiveCard> unitCards() {
            return activeCards.FindAll(x => !string.IsNullOrEmpty(x.baseSpec.unit.name));
        }

        public List<ActiveCard> spellCards() {
            return activeCards.FindAll(x => !string.IsNullOrEmpty(x.baseSpec.skill.name));
        }
    }
}

[System.Serializable]
public class ActiveCard {
    public int id;
    public GameObject gameObject;
    public BaseSpec baseSpec = new BaseSpec();
    public string type;
    public Ev ev = new Ev();

    public void ChangeHp(int newVal) { ev.hp = newVal; }
    public void TakeDamage(int amount) {
        int changedHp = ev.hp - amount;
        ChangeHp(changedHp);
        //Debug.Log("데미지 받음");
    }
}