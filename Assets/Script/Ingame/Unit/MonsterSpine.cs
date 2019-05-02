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

    public override void Move() {
        if (CheckOverlap(runAnimationName)) return;
        spineAnimationState.SetAnimation(0, runAnimationName, true);
        currentAnimationName = runAnimationName;
    }

    public override void Idle() {
        if (CheckOverlap(idleAnimationName)) return;
        spineAnimationState.SetAnimation(0, idleAnimationName, true);
        currentAnimationName = idleAnimationName;
    }
}
