using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Container {
    public class PlayerActiveCards : MonoBehaviour {
        public List<ActiveCard> activeCards;
        public List<ActiveCard> opponentCards;
        [SerializeField] [ReadOnly] DeckInfo deckInfo;
        [SerializeField] [ReadOnly] PlayerController playerController;

        public void Init() {
            playerController = PlayerController.Instance;
            DataModules.MissionData missionData;
            DataModules.Deck playerDeck;
            DataModules.Deck opponentDeck;
            ConstructManager constructManager = ConstructManager.Instance;
            
            /*
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
            */
            
            try {
                missionData = AccountManager.Instance.mission;
                playerDeck = missionData.playerDeck;
                opponentDeck = missionData.opponentDeck;
            }
            catch (NullReferenceException ex) {
                Debug.LogError("미션데이터가 로드 되지않았습니다.");
                return;
            }
            activeCards = new List<ActiveCard>();
            opponentCards = new List<ActiveCard>();

            for(int i = 0; i < playerDeck.cards.Length; i++) {
                DataModules.CardData cardData = playerDeck.cards[i].data;
                ActiveCard activeCard = new ActiveCard();
                
                if(cardData != null) {
                    switch (cardData.type) {
                        case "active":
                            activeCard.id = playerDeck.cards[i].id;
                            activeCard.baseSpec.skill = cardData.activeSkills[0];
                            activeCard.baseSpec.unit = new DataModules.Unit();
                            activeCard.type = cardData.type;
                            break;
                        case "hero":
                            activeCard.id = playerDeck.cards[i].id;
                            activeCard.baseSpec.skill = new DataModules.ActiveSkill();
                            activeCard.baseSpec.unit = cardData.unit;
                            activeCard.type = cardData.type;
                            break;
                    }
                    activeCards.Add(activeCard);
                }               
            }
            
            for (int i = 0; i < opponentDeck.cards.Length; i++) {
                DataModules.CardData cardData = opponentDeck.cards[i].data;
                ActiveCard activeCard = new ActiveCard();

                if(cardData != null) {
                    switch (cardData.type) {
                        case "active":
                            activeCard.id = opponentDeck.cards[i].id;
                            activeCard.baseSpec.skill = cardData.activeSkills[0];
                            activeCard.baseSpec.unit = new DataModules.Unit();
                            activeCard.type = cardData.type;
                            break;
                        case "hero":
                            activeCard.id = opponentDeck.cards[i].id;
                            activeCard.baseSpec.skill = new DataModules.ActiveSkill();
                            activeCard.baseSpec.unit = cardData.unit;
                            activeCard.type = cardData.type;
                            break;
                    }
                    opponentCards.Add(activeCard);
                }
            }

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