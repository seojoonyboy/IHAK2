using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Container {
    public class PlayerResource : MonoBehaviour {
        [ReadOnly][Tooltip("Show current Gold In Inspector\nOrigin Variable is decimal")] public float gold_readonly;
        [ReadOnly][Tooltip("Show current Env In Inspector\nOrigin Variable is decimal")] public float env_readonly;

        private decimal gold;
        private decimal env;
        public int maxhp;
        [ReadOnly] public int hp;

        public PlayerResource(int gold, int env) {
            Gold = gold;
            Env = env;
        }

        public decimal Gold {
            get { return gold; }
            set {
                if (gold < value) {
                    Debug.LogError("자원이 0이하로 떨어졌습니다.");
                    gold = 0;
                }
                gold = value;

                gold_readonly = (float)Gold;
            }
        }

        public decimal Env {
            get { return env; }
            set {
                env = value;
                Debug.Log("환경 감소");

                env_readonly = (float)Env;
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
    }
}