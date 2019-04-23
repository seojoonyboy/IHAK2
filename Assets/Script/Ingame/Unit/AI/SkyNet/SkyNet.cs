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
        [SerializeField] private int expPoint;
        private int lastAttackLayer = 0;


        protected static int myLayer = 0;
        protected static int enemyLayer = 0;
        protected static int neutralLayer = 0;

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

        public virtual void Damage(float damage, Transform enemy) {
            Damage(damage);
            lastAttackLayer = enemy.gameObject.layer;
        }

        public int ThisPlayerHitMe() {
            return lastAttackLayer;
        }

        /// <summary>
        /// 현재 체력을 정수만큼 회복
        /// </summary>
        /// <param name="amount">정수 회복량</param>
        public virtual void Recover(float amount) {
            HP += amount;
        }

        public virtual void Die() { }
        protected virtual void GiveExp() {
            int layerToGive = ThisPlayerHitMe();
            if(layerToGive == neutralLayer) return;
            List<HeroAI> heroes = new List<HeroAI>();
            FindCloseHero(heroes, layerToGive);
            if (heroes.Count == 0) return;
            int exp = expPoint / heroes.Count;
            for (int i = 0; i < heroes.Count; i++) heroes[i].ExpGain(exp);
        }

        private void FindCloseHero(List<HeroAI> heroes, int layer) {
            float ExpGiveLength = 20f;
            GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
            for (int i = 0; i < units.Length; i++) {
                if (units[i].layer != layer) continue;
                if (units[i].GetComponent<HeroAI>() == null) continue;
                float length = Vector3.Distance(units[i].transform.position, transform.position);
                if (length > ExpGiveLength) continue;
                heroes.Add(units[i].GetComponent<HeroAI>());
            }
        }

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

        protected int LayertoGive(bool isEnemy) {
            if(gameObject.layer == myLayer)
                return isEnemy ?  (1 << enemyLayer) | (1 << neutralLayer) : myLayer;
            else
                return isEnemy ? (1 << myLayer) | (1 << neutralLayer) : enemyLayer;
        }
    }   
}