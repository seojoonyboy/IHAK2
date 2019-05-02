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

        void Start() {
            Debug.Log("시작 시민과 골드 보정");
            Gold = 300;
            Citizen = 300;
        }

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
                if(value > 1000) { value = 1000; return; }
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
                if (value > 1000) { value = 1000; return; }
                citizen = value;
                PlayerController.Instance.resourceManager().RefreshCitizen();
                PlayerController.Instance.CitizenSpawnController().AddCitizen();

                citizen_readonly = (float)citizen;
            }
        }

        public void UseGold(decimal amount = 0) {
            Gold -= amount * 100;
        }

        public void UseCitizen(decimal amount = 0) {
            Citizen -= amount * 100;
        }
    }
}