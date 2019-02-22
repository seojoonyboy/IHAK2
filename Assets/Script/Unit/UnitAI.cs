using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DataModules;
using System;
using PolyNav;

public class UnitAI : MonoBehaviour {
	public enum aiState {
		NONE,
		MOVE,
		ATTACK,
		DEAD
	};
    public Sprite enemyGauge;
    public Sprite playerGauge;

	private delegate void timeUpdate(float time);
	private timeUpdate update;
	private Transform healthBar;
	private IngameCityManager.BuildingInfo target;
	private float maxHealth = 0;
    public float health = 0;
	private float moveSpeed;
	private float currentTime;
	[SerializeField]
	private Unit unit;

	private static IngameCityManager cityManager;

	private SkeletonAnimation skeletonAnimation;
	public Spine.AnimationState spineAnimationState;
    public Spine.Skeleton skeleton;

    public GameObject ontile;
	public bool protecting = false;
	private PolyNavAgent agent;

	private List<IngameCityManager.BuildingInfo> buildingInfos;
	private IngameCityManager.Target targetEnum;

	void Start () {
		healthBar = transform.GetChild(1).GetChild(1);
		if(cityManager == null) cityManager = FindObjectOfType<IngameCityManager>();
		skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
		spineAnimationState = skeletonAnimation.AnimationState;
		skeleton = skeletonAnimation.Skeleton;
		agent = GetComponent<PolyNavAgent>();
		if(gameObject.layer == LayerMask.NameToLayer("PlayerUnit")) {
			buildingInfos = cityManager.enemyBuildingsInfo;
            SpriteRenderer unitgaugeColor = transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>();
            unitgaugeColor.sprite = playerGauge;
            targetEnum = IngameCityManager.Target.ENEMY_1;
		}
		if(gameObject.layer == LayerMask.NameToLayer("EnemyUnit")) {
			buildingInfos = cityManager.myBuildingsInfo;
            SpriteRenderer unitgaugeColor = transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>();
            unitgaugeColor.sprite = enemyGauge;
            targetEnum = IngameCityManager.Target.ME;
		}
		if(searchTarget())
			setState(aiState.MOVE);
		else
			setState(aiState.NONE);
		IngameSceneEventHandler.Instance.AddListener(IngameSceneEventHandler.EVENT_TYPE.UNIT_UPGRADED, UnitUpgrade);
	}

	private void OnDestroy() {
		IngameSceneEventHandler.Instance.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.UNIT_UPGRADED, UnitUpgrade);
	}

    public void SetUnitData(Unit unit) {
		this.unit = unit;
		moveSpeed = unit.moveSpeed;
		float temphealth = unit.hitPoint - maxHealth;
		maxHealth = unit.hitPoint;
        health += temphealth;
		//agent.maxSpeed = moveSpeed;
	}

    private void UnitUpgrade(Enum Event_Type, Component Sender, object Param) {
        Unit unit = (Unit)Param;
		if(this.unit.id.CompareTo(unit.id) != 0) return;
		SetUnitData(unit);
		calculateHealthBar();
    }

	private void setState(aiState state) {
		update = null;
		currentTime = 0f;
		switch(state) {
			case aiState.NONE :
			spineAnimationState.SetAnimation(0, "stand", true);
			update = noneUpdate;
			break;
			case aiState.MOVE :
			spineAnimationState.SetAnimation(0, "run", true);
			update = moveUpdate;
			agent.SetDestination(target.gameObject.transform.parent.position);
			Debug.Log(target.gameObject.transform.parent.parent);
			break;
			case aiState.ATTACK :
			spineAnimationState.SetAnimation(0, "stand", true);
			update = attackUpdate;
			break;
			case aiState.DEAD :
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
        Vector3 buildingPos = target.gameObject.transform.parent.localPosition;
		Vector3 distance = buildingPos - transform.localPosition;
        float length = Vector3.Distance(transform.localPosition, buildingPos);
		setFlip(distance);
		if(isBuildingClose(length)) {
			setState(aiState.ATTACK);
			agent.Stop();
			return;
		}
		//Vector3 force = distance.normalized * moveSpeed * time;
		//transform.Translate(force.x, force.y, 0f);
		if(currentTime < 2f) return;
		agent.SetDestination(target.gameObject.transform.parent.position);
		currentTime = 0f;
		searchBuilding();
	}

	void attackUpdate(float time) {
		currentTime += time;
		IngameCityManager.BuildingInfo building = target;
		if(currentTime < unit.attackSpeed) return;
		currentTime = 0f;
		cityManager.TakeDamage(targetEnum, target.tileNum, unit.power);
		if(building.hp <= 0) {
            target = null;
			if(searchTarget())
				setState(aiState.MOVE);
			else
				setState(aiState.NONE);
		}
		spineAnimationState.SetAnimation(0, "attack", false);
		spineAnimationState.AddAnimation(0, "stand", true, 0);
	}

	private bool isBuildingClose(float distance) {
		if(target == null) {
			searchTarget();
			return false;
		}
		if(distance <= unit.attackRange) 
			return true;
		return false;
	}

	private bool searchTarget() {
		if(protecting) {
			return false;
		}
		else {
			searchBuilding();
			return target != null;
		}
	}

	private void searchBuilding() {
		float distance = 0f;
		foreach(IngameCityManager.BuildingInfo target in buildingInfos) {
			if(target.hp <= 0) continue;
			
			Vector3 buildingPos = target.gameObject.transform.parent.position;
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
		calculateHealthBar();
		if(health <= 0) {            
            //ontile.GetComponent<TileCollision>().count--;
            if (ontile != null && ontile.GetComponent<TileCollision>().count <= 0)
                ontile.GetComponent<TileCollision>().check = false;
            Destroy(gameObject);
		}
	}

	private void calculateHealthBar() {
		if(!healthBar.parent.gameObject.activeSelf) healthBar.parent.gameObject.SetActive(true);
		float percent = (float)health / maxHealth;
		healthBar.transform.localScale = new Vector3(percent, 1f, 1f);
	}

	void setFlip(Vector2 move) {
        skeleton.ScaleX = move.x < 0 ? 1f: -1f;
	}

    public void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.tag == "Tile")
            coll.GetComponent<TileCollision>().count++;
    }


    public void OnTriggerStay2D(Collider2D coll) {
        if (coll.gameObject.tag == "Tile") {
            ontile = coll.gameObject;
            ontile.GetComponent<TileCollision>().check = true;
        }
    }

    public void OnTriggerExit2D(Collider2D coll) {
        if (coll.gameObject.tag == "Tile") {
            if (coll.GetComponent<TileCollision>().count > 0) {
                coll.GetComponent<TileCollision>().count--;

                if (coll.GetComponent<TileCollision>().count == 0)
                    coll.GetComponent<TileCollision>().check = false;
            }
        }
    }

}
