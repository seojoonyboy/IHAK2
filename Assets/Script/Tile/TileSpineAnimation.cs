using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class TileSpineAnimation : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start() {
		SkeletonAnimation ani = GetComponent<SkeletonAnimation>();
		if(ani == null) yield break;
		SkeletonDataAsset skeleton = GetComponent<BuildingObject>().spine;
		skeleton.GetSkeletonData(false);
		yield return new WaitForSeconds(0.1f);
		var data = skeleton.GetSkeletonData(false).Animations.Items[0];
        ani.skeletonDataAsset = skeleton;
        ani.Initialize(false);
		ani.skeleton.SetAttachment("tile", null);
        ani.AnimationState.SetAnimation(0, data, true);
	}
}
