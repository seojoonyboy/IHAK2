using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class UnitSpine : MonoBehaviour {
    [SpineAnimation]
    public string runUpAnimationName;
    [SpineAnimation]
    public string runDownAnimationName;
    [SpineAnimation]
    public string idleUpAnimationName;
    [SpineAnimation]
    public string idleDownAnimationName;
    [SpineAnimation]
    public string attackUpAnimationName;
    [SpineAnimation]
    public string attackDownAnimationName;
    [SpineAnimation]
    public string hitUpAnimationName;
    [SpineAnimation]
    public string hitDownAnimationName;

    private bool isUp;

    private SkeletonAnimation skeletonAnimation;
    private Spine.AnimationState spineAnimationState;
    private Spine.Skeleton skeleton;
    
    [SerializeField]
    private float overrideMixDuration = 0.15f;

    private void Awake() {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.AnimationState;
        skeleton = skeletonAnimation.Skeleton;
        spineAnimationState.Data.DefaultMix = overrideMixDuration;
    }

    public void Idle() {
        if(isUp) spineAnimationState.SetAnimation(0, idleUpAnimationName, true);
        else spineAnimationState.SetAnimation(0, idleDownAnimationName, true);
    }

    public void Move() {
        if(isUp) spineAnimationState.SetAnimation(0, runUpAnimationName, true);
        else spineAnimationState.SetAnimation(0, runDownAnimationName, true);
    }

    public void Attack() {
        Spine.TrackEntry entry;
        if(isUp) entry = spineAnimationState.SetAnimation(0, attackUpAnimationName, false);
        else entry = spineAnimationState.SetAnimation(0, attackDownAnimationName, false);
        Invoke("Idle", entry.TrackEnd);
    }

    public void Hitted() {
        spineAnimationState.SetEmptyAnimation(1, 0);
        if(isUp) spineAnimationState.AddAnimation(0, hitUpAnimationName, true, 0f);
        else spineAnimationState.AddAnimation(0, hitDownAnimationName, true, 0f);
        spineAnimationState.AddEmptyAnimation(1, overrideMixDuration, 0);
    }

    public void SetDirection(Vector2 direction) {
        skeleton.ScaleX = direction.x < 0 ? -1f: 1f;
        isUp = direction.y > 0;
    }	
}
