using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DataModules;

public class UnitAI : MonoBehaviour {
	public enum enemyState {
		NONE,
		MOVE,
		ATTACK,
		DEAD
	};
	private delegate void timeUpdate(float time);
	private timeUpdate update;
	private SpriteRenderer healthBar;
	private BuildingObject target;
	private float maxHealth;
    private float health;
	private float moveSpeed;
	private float currentTime;
	private Unit unit;

	private static IngameCityManager cityManager;

	private SkeletonAnimation skeletonAnimation;
	public Spine.AnimationState spineAnimationState;
    public Spine.Skeleton skeleton;
	
	void Start () {
		SearchUnitData();
		//healthBar = transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
		moveSpeed = 1f / unit.moveSpeed;
		maxHealth = unit.hitPoint;
        health = unit.hitPoint;

		skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
		spineAnimationState = skeletonAnimation.AnimationState;
		skeleton = skeletonAnimation.Skeleton;

		searchBuilding();
		setState(enemyState.MOVE);
	}
	/// <summary>
	/// 임시여서 나중에는 제대로 된 unit입력을 받아야함.
	/// </summary>
	private void SearchUnitData() {
		TileGroup[] list = FindObjectsOfType<TileGroup>();
		foreach(TileGroup tilegroup in list) {
            if (tilegroup.gameObject.name.CompareTo("TileGroup_Empty_1(Clone)") == 0)
                unit = tilegroup.units[0];
		}
		if(cityManager == null) cityManager = FindObjectOfType<IngameCityManager>();
	}

	private void setState(enemyState state) {
		update = null;
		currentTime = 0f;
		switch(state) {
			case enemyState.NONE :
			spineAnimationState.SetAnimation(0, "stand", true);
			update = noneUpdate;
			break;
			case enemyState.MOVE :
			spineAnimationState.SetAnimation(0, "run", true);
			update = moveUpdate;
			break;
			case enemyState.ATTACK :
			spineAnimationState.SetAnimation(0, "stand", true);
			update = attackUpdate;
			break;
			case enemyState.DEAD :
			update = noneUpdate;
			break;
		}
	}

	void Update() {
		update(Time.deltaTime);
	}

	void noneUpdate(float time) {
		return;
	}

	void moveUpdate(float time) {
		currentTime += time;
        Vector3 buildingPos = target.transform.parent.localPosition;
		Vector3 distance = buildingPos - transform.localPosition;
        float length = Vector3.Distance(transform.localPosition, buildingPos);
		setFlip(distance);
		if(isBuildingClose(length)) {
			setState(enemyState.ATTACK);
			return;
		}
		Vector3 force = distance.normalized * moveSpeed * time * 50f;
		//transform.position += force;
		transform.Translate(force.x, force.y, 0f);
		if(currentTime < 2f) return;
		currentTime = 0f;
		searchBuilding();
	}

	void attackUpdate(float time) {
		currentTime += time;
		int tileNum = target.GetComponentInParent<TileObject>().tileNum;
		IngameCityManager.BuildingInfo enemy = cityManager.enemyBuildingsInfo[tileNum];
		if(currentTime < unit.attackSpeed) return;
		currentTime = 0f;
		cityManager.TakeDamage(IngameCityManager.Target.ENEMY_1, tileNum, unit.power);
		if(!enemy.activate && enemy.hp <= 0) {
			searchBuilding(); 
			setState(enemyState.MOVE);};
		spineAnimationState.SetAnimation(0, "attack", false);
		spineAnimationState.AddAnimation(0, "stand", true, 0);
	}

	private bool isBuildingClose(float distance) {
		if(target == null) {
			searchBuilding();
			return false;
		}
		if(distance <= unit.attackRange * 2f) 
			return true;
		return false;
	}

	private void searchBuilding() {
		BuildingObject[] buildings = FindObjectsOfType<BuildingObject>();
		if(buildings.Length == 0) {
			setState(enemyState.NONE);
		}
		float distance = 0f;
		foreach(BuildingObject target in buildings) {
			//TODO : 해당 건물의 HP가 0인지를 파악해야함
			int num = target.transform.GetComponentInParent<TileObject>().tileNum;
			if(cityManager.enemyBuildingsInfo[num].hp <= 0) continue;
			Vector3 buildingPos = target.transform.parent.position;
			float length = Vector3.Distance(transform.position, buildingPos);
			if(this.target == null) {
				this.target = target;
				distance = length;
				continue;
			}
			if(distance > length) {
				this.target = target;
				distance = length;
				continue;
			}
		}
	}

	public void damaged(int damage) {
		health -= damage;
		//calculateHealthBar();
		if(health <= 0) {
			Destroy(gameObject);
		}
	}

	private void calculateHealthBar() {
		float percent = (float)health / maxHealth;
		float sizeWidth = 2.5f * percent;
		float posX = percent * 1.25f - 1.25f;
		healthBar.size = new Vector2(sizeWidth, 0.3f);
		healthBar.transform.localPosition = new Vector3(posX, 0f, 0f);
	}

	void setFlip(Vector2 move) {
        //bool isFlip = move.y < 0 || (move.y == 0 && move.x > 0);
        //skeleton.ScaleX = isFlip ? 1f: -1f;
        skeleton.ScaleX = move.x < 0 ? 1f: -1f;
	}
}
