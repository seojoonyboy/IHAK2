using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI_submodule {
    public class Patrol : MonoBehaviour {
        public CreepStation tower;
        public Vector2 patrolTarget;
        List<Transform> waypoints;

        float time;
        float speed = 10;
        float interval = 8;

        // Update is called once per frame
        void Update() {
            if (patrolTarget == null) return;
            time += Time.deltaTime;

            transform.position = Vector2.MoveTowards(
                new Vector2(transform.position.x, transform.position.y),
                patrolTarget,
                speed * Time.deltaTime
            );

            if (this.time > interval) {
                patrolTarget = GetPatrolTarget();
                this.time = 0;
                interval = 10 + Random.Range(-1.0f, 1.0f);
            }
        }

        public void Init(List<Transform> waypoints) {
            this.waypoints = waypoints;
            patrolTarget = GetPatrolTarget();
            tower = GetComponent<MonsterAI>().tower;
        }

        private Vector2 GetPatrolTarget() {
            if (tower == null) return transform.position;
            int rndNum = Random.Range(0, waypoints.Count - 1);

            Transform target = tower.transform.GetChild(0).GetChild(rndNum).transform;
            float offsetX = Random.Range(-10.0f, 10.0f);
            float offsetY = Random.Range(-5.0f, 5.0f);
            Vector2 vector = new Vector2(target.position.x + offsetX, target.position.y + offsetY);
            return vector;
        }
    }
}
