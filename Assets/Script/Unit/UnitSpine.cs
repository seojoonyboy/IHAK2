using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

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
    private Transform hitEffect;

    private SkeletonAnimation skeletonAnimation;
    private Spine.AnimationState spineAnimationState;
    private Skeleton skeleton;

    private void Awake() {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.AnimationState;
        skeleton = skeletonAnimation.Skeleton;
        hitEffect = transform.parent.Find("wolf_hit");
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
        if(isUp) spineAnimationState.SetAnimation(1, hitUpAnimationName, false);
        else spineAnimationState.SetAnimation(1, hitDownAnimationName, false);
        StartCoroutine("SetColor");
    }

    public void SetDirection(Vector2 direction) {
        skeleton.ScaleX = direction.x < 0 ? -1f: 1f;
        isUp = direction.y > 0;
    }

    IEnumerator SetColor() {
        hitEffect.gameObject.SetActive(true);
        skeleton.SetColor(Color.red);
        yield return new WaitForSeconds(0.25f);
        skeleton.SetColor(Color.white);
        yield return new WaitForSeconds(0.25f);
        hitEffect.gameObject.SetActive(false);
    }
}
