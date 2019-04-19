using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    /// <summary>
    /// Mother Of All AI System
    /// </summary>
    public abstract class SkyNet : MonoBehaviour {
        private float hp;
        private float maxHp;
        public PlayerController.Player ownerNum;
        protected Transform healthBar;

        public float HP {
            get {
                return hp;
            }
            set {
                hp = value;
                if(hp < 0) {
                    Die();
                    hp = 0;
                }
                if(hp > maxHp) {
                    hp = maxHp;
                }
                CalculateHealthBar();
            }
        }

        public float MaxHealth {
            get { return maxHp; }
        }

        /// <summary>
        /// 기본 Data 초기화
        /// </summary>
        protected abstract void Init();

        /// <summary>
        /// 정수 피해량 만큼 체력 감소
        /// </summary>
        /// <param name="amount">정수 피해량</param>
        public virtual void Damage(float amount) {
            HP -= amount;
        }

        /// <summary>
        /// 현재 체력을 정수만큼 회복
        /// </summary>
        /// <param name="amount">정수 회복량</param>
        public virtual void Recover(float amount) {
            HP += amount;
        }

        protected virtual void Die() { }
        protected virtual void GainExp() { }
        protected virtual void LvUp() { }
        protected virtual void CalculateHealthBar() { }
        public UnitGroup GetMyUnitGroup() {
            if(GetComponent<UnitGroup>() == null) {
                Debug.LogError(gameObject.name + "의 UnitGroup 컴포넌트를 찾을 수 없습니다!");
                return null;
            }
            else {
                return GetComponent<UnitGroup>();
            }
        }

        public virtual void ChangeOwner(int newNum) {
            ownerNum = (PlayerController.Player)newNum;
        }
    }
}