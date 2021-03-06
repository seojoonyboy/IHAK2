using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public abstract class Action : ScriptableObject {
        public abstract void Act(StateController controller);
        public abstract void TimeReset(StateController controller);
    }
}