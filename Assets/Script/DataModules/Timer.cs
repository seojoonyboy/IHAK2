using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataModules {
    public class Timer : MonoBehaviour {
        public float time;
        // Use this for initialization
        void Start() {
            time = GetComponent<StateController>().time;
        }
    }
}
