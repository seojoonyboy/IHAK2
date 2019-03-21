using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class TileSpineAnimation : MonoBehaviour {
	
	SkeletonAnimation ani;
	SkeletonDataAsset skeleton;
	Spine.Animation[] items;
	int level = 0;
	// Use this for initialization
	IEnumerator Start() {
		ani = GetComponent<SkeletonAnimation>();
		if(ani == null) yield break;
		skeleton = GetComponent<BuildingObject>().spine;
		skeleton.GetSkeletonData(false);
		yield return new WaitForSeconds(0.1f);
		items = skeleton.GetSkeletonData(false).Animations.Items;
        ani.skeletonDataAsset = skeleton;
        ani.Initialize(false);
        ani.AnimationState.SetAnimation(0, items[level], true);
		/*try {
			ani.skeleton.SetAttachment("tile", null);
		} 
		catch (System.Exception error) {
			Debug.Log(error + "\n이 스파인은 바닥이 없는 스파인입니다.");
		}*/
		transform.localPosition = Vector3.zero;
	}

	public void Upgrade() {
		level++;
		if(items.Length <= level) {
			Debug.Log(name+"의 해당 레벨의 이미지는 존재하지 않습니다.");
			return;
		}
		ani.AnimationState.SetAnimation(0, items[level], true);
	}

	public void SetUnit(bool on) {
		var slots = ani.skeleton.Slots;
		for (int i = 0, n = slots.Count; i < n; i++) {
			Slot slot = slots.Items[i];
			string name = slot.Data.Name;
			if (name != "room") ani.skeleton.SetAttachment(name, on ? name : null);
		}
	}
}
