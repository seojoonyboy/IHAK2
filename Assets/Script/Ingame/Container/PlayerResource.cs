using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Container {
    public class PlayerResource : MonoBehaviour {
        [ReadOnly][Tooltip("Show current Gold In Inspector\nOrigin Variable is decimal")] public float gold_readonly;
        [ReadOnly][Tooltip("Show current Env In Inspector\nOrigin Variable is decimal")] public float citizen_readonly;

        private decimal gold;
        private decimal citizen;
        
        public int maxhp;
        [ReadOnly] public int hp;

        public PlayerResource(int gold, int citizen) {
            Gold = gold;
            Citizen = citizen;
        }

        public decimal Gold {
            get { return gold; }
            set {
                if (value < 0 && gold < value) {
                    gold = 0;
                }
                if(value > 100) { value = 100; return; }
                gold = value;
                PlayerController.Instance.resourceManager().RefreshGold();

                gold_readonly = (float)Gold;
            }
        }

        public decimal Citizen {
            get { return citizen; }
            set {
                if (value < 0 && citizen < value) {
                    Debug.LogError("시민이 0이하로 떨어졌습니다.");
                    citizen = 0;
                }
                if (value > 100) { value = 100; return; }
                citizen = value;
                PlayerController.Instance.resourceManager().RefreshCitizen();
                PlayerController.Instance.CitizenSpawnController().AddCitizen();

                citizen_readonly = (float)citizen;
            }
        }

        public int TotalHp {
            get { return hp; }
            set {
                hp = value;
                if (hp <= 0) {
                    Debug.Log("도시체력 0이하로 떨어짐");
                    hp = 0;
                }
            }
        }

        public void UseGold(decimal amount = 0) {
            Gold -= amount * 10;
        }

        public void UseCitizen(decimal amount = 0) {
            Citizen -= amount * 10;
        }
    }
}