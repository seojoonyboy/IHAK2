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
                transform.position = Vector2.MoveTowards(
                    new Vector2(transform.position.x, transform.position.y),
                    target.position,
                    speed * Time.deltaTime
                );
            }
            else {
                stateController.TransitionToState(stateController.allStates[1]);
            }
        }

        public void Init(StateController stateController) {
            this.stateController = stateController;
        }

        void OnTriggerEnter2D(Collider2D collision) {
            if ((collision.gameObject.layer != 14) && collision.GetComponent<UnitAI>() != null) {
                stateController.TransitionToState(stateController.allStates[0]);
                isCloseToTarget = true;
            }
        }

        void OnTriggerExit2D(Collider2D collision) {
            if ((collision.gameObject.layer != 14) && collision.GetComponent<UnitAI>() != null) {
                isCloseToTarget = false;
            }
        }
    }
}