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
    [SpineAnimation]
    public string skillAnimationName;
    protected string currentAnimationName;
    //[SpineAnimation]
    //public string hittedAnimationName;
    private Transform hitEffect;

    [SpineSkin]
    public string skinBlueSkull;
    [SpineSkin]
    public string skinRedSkull;

    protected SkeletonAnimation skeletonAnimation;
    protected Spine.AnimationState spineAnimationState;
    protected Skeleton skeleton;

    private void Awake() {
        Init();
        hitEffect = transform.parent.Find("wolf_hit");
    }

    private void Start() {
        SetSkin();
    }

    public virtual void Init() {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.AnimationState;
        skeleton = skeletonAnimation.Skeleton;
    }

    public virtual void SetSkin() {
        string skin = gameObject.layer == LayerMask.NameToLayer("PlayerUnit") ? skinBlueSkull : skinRedSkull;
        skeleton.SetSkin(skin);
    }

    public virtual void Idle() {
        //if(CheckOverlap(idleAnimationName)) return;
        spineAnimationState.SetAnimation(0, idleAnimationName, true);
        currentAnimationName = idleAnimationName;
    }

    public virtual void Move() {
        if(CheckOverlap(runAnimationName)) return;
        spineAnimationState.SetAnimation(0, runAnimationName, true);
        currentAnimationName = idleAnimationName;
    }

    public void Attack() {
        //if(CheckOverlap(attackAnimationName)) return;
        Spine.TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, attackAnimationName, false);
        currentAnimationName = attackAnimationName;
        Invoke("Idle", entry.TrackEnd);
    }

    public bool CheckOverlap(string name) {
        if(name.CompareTo(currentAnimationName) == 0) return true;
        return false;
    }

    public void Hitted() {
        //spineAnimationState.SetAnimation(1, hittedAnimationName, false);
        StartCoroutine("SetColor");
    }

    public void SetDirection(Vector2 direction) {
        skeleton.ScaleX = direction.x >= 0 ? -1f: 1f;
    }

    public void Skill() {
        if(string.IsNullOrEmpty(skillAnimationName)) return;
        Spine.TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, skillAnimationName, false);
        Invoke(currentAnimationName, entry.TrackEnd);
        currentAnimationName = skillAnimationName;
        
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
