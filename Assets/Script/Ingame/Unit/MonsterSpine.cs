using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public class MonsterSpine : UnitSpine {
    void Awake() {
        base.Init();
    }

    public override void SetSkin() {
        skeleton.SetColor(Color.gray);
    }
}
