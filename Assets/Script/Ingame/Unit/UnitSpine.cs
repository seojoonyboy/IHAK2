using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public class UnitSpine : MonoBehaviour {
    [SpineAnimation]
    public string runAnimationName;
    [SpineAnimation]
    public string idleAnimationName;
    [SpineAnimation]
    public string attackAnimationName;
    //[SpineAnimation]
    //public string hittedAnimationName;
    private Transform hitEffect;

    private SkeletonAnimation skeletonAnimation;
    private Spine.AnimationState spineAnimationState;
    private Skeleton skeleton;

    private void Awake() {
        Init();
        hitEffect = transform.parent.Find("wolf_hit");
    }

    public virtual void Init() {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.AnimationState;
        skeleton = skeletonAnimation.Skeleton;
    }

    public void Idle() {
        spineAnimationState.SetAnimation(0, idleAnimationName, true);
    }

    public void Move() {
        spineAnimationState.SetAnimation(0, runAnimationName, true);
    }

    public void Attack() {
        Spine.TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, attackAnimationName, false);
        Invoke("Idle", entry.TrackEnd);
    }

    public void Hitted() {
        //spineAnimationState.SetAnimation(1, hittedAnimationName, false);
        StartCoroutine("SetColor");
    }

    public void SetDirection(Vector2 direction) {
        skeleton.ScaleX = direction.x < 0 ? -1f: 1f;
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
