using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    /// <summary>
    /// Mother Of All AI System
    /// </summary>
    public abstract class SkyNet : MonoBehaviour {
        [SerializeField] private float hp;
        [SerializeField] private float maxHp;
        public PlayerController.Player ownerNum;
        [SerializeField] protected Transform healthBar;

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
            get {
                return maxHp;
            }

            set {
                maxHp = value;
            }
        }

        /// <summary>
        /// 기본 Data 초기화
        /// </summary>
        public abstract void Init(object data);

        public abstract void Init(object data, GameObject gameObject);
        //public abstract void SetUnitGroup();

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

        public virtual void Die() { }
        protected virtual void GainExp() { }
        protected virtual void LvUp() { }
        protected virtual void CalculateHealthBar() {
            if (healthBar == null) {
                Debug.LogError(gameObject.name + "의 HealthBar가 설정되어 있지 않습니다.");
                return;
            }
            float percent = HP / MaxHealth;
            if(MaxHealth != 0) {
                healthBar.transform.localScale = new Vector3(percent, 1f, 1f);
            }
        }

        public virtual void ChangeOwner(int newNum) {
            ownerNum = (PlayerController.Player)newNum;
        }
    }
}