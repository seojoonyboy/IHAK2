using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI_submodule {
    public class MoveToAttack : MonoBehaviour {
        Transform target;
        StateController stateController;
        float speed = 10;
        public bool isCloseToTarget;
        // Update is called once per frame
        void Update() {
            if (stateController == null) return;
            target = stateController.chaseTarget;
            if (target != null) {
                float dist = Vector2.Distance(target.position, transform.position);
                if(dist >= 8.0f) {
                    transform.position = Vector2.MoveTowards(
                    new Vector2(transform.position.x, transform.position.y),
                    target.position,
                    speed * Time.deltaTime
                );
                }
            }
            else {
                stateController.TransitionToState(stateController.allStates[1]);
            }
        }

        public void Init(StateController stateController) {
            this.stateController = stateController;
        }

        void OnTriggerStay2D(Collider2D collision) {
            if ((collision.gameObject.layer != 14) && collision.GetComponent<UnitAI>() != null) {
                stateController.TransitionToState(stateController.allStates[0]);
                if(GetComponent<Timer>() == null) {
                    gameObject.AddComponent<Timer>();
                }
                isCloseToTarget = true;
            }
        }

        void OnTriggerExit2D(Collider2D collision) {
            if ((collision.gameObject.layer != 14) && collision.GetComponent<UnitAI>() != null) {
                if (GetComponent<Timer>() != null) {
                    Destroy(GetComponent<Timer>());
                }
                isCloseToTarget = false;
            }
        }
    }
}