using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAniSet : MonoBehaviour {
    public enum ButtonState {
        touch,
        Inactive,
        activation,
    }

    private SkeletonGraphic skeletonAnimation;
    public Spine.AnimationState spineAnimationState;
    public Spine.Skeleton skeleton;
    ButtonState nowState;

    private void Awake() {

    }

    private void Start() {
        skeletonAnimation = GetComponentInChildren<SkeletonGraphic>();
        spineAnimationState = skeletonAnimation.AnimationState;
        skeleton = skeletonAnimation.Skeleton;
        nowState = ButtonState.Inactive;
    }

    public void SetState(ButtonState state) {
        switch (state) {
            case ButtonState.touch:
                spineAnimationState.SetAnimation(0, "1.touch", false);
                SetState(ButtonState.activation);
                break;
            case ButtonState.Inactive:
                spineAnimationState.SetAnimation(0, "2.Inactive", false);
                break;
            case ButtonState.activation:
                spineAnimationState.AddAnimation(0, "3.activation", true, 0.8f);
                break;
        }
    }

}
