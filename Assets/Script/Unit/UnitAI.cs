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
	private IngameCityManager.BuildingInfo targetBuilding;
	private UnitAI targetUnit;
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
	private CircleCollider2D detectCollider;

	private List<IngameCityManager.BuildingInfo> buildingInfos;
	private IngameCityManager.Target targetEnum;

	void Start () {
		healthBar = transform.GetChild(1).GetChild(1);
		if(cityManager == null) cityManager = FindObjectOfType<IngameCityManager>();
		skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
		spineAnimationState = skeletonAnimation.AnimationState;
		skeleton = skeletonAnimation.Skeleton;
		agent = GetComponent<PolyNavAgent>();
		detectCollider = transform.GetComponentInChildren<CircleCollider2D>();
		detectCollider.radius = unit.detectRange;
		if(gameObject.layer == LayerMask.NameToLayer("PlayerUnit")) {
			buildingInfos = cityManager.enemyBuildingsInfo;
            SpriteRenderer unitgaugeColor = transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>();
            unitgaugeColor.sprite = playerGauge;
            targetEnum = IngameCityManager.Target.ENEMY_1;
			GetComponentInChildren<UnitDetector>().detectingLayer = LayerMask.NameToLayer("EnemyUnit");
            GetComponentInChildren<UnitDetector>().gameObject.layer = LayerMask.NameToLayer("PlayerUnit");

        }
		if(gameObject.layer == LayerMask.NameToLayer("EnemyUnit")) {
			buildingInfos = cityManager.myBuildingsInfo;
            SpriteRenderer unitgaugeColor = transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>();
            unitgaugeColor.sprite = enemyGauge;
            targetEnum = IngameCityManager.Target.ME;
			GetComponentInChildren<UnitDetector>().detectingLayer = LayerMask.NameToLayer("PlayerUnit");
            GetComponentInChildren<UnitDetector>().gameObject.layer = LayerMask.NameToLayer("EnemyUnit");
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
			agent.maxSpeed = unit.moveSpeed;
			update = moveUpdate;
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

        if (health <= 0) {
            DestoryEnemy();
        }
    }

	void noneUpdate(float time) {
		currentTime += time;
		if((int)currentTime % 3 == 2)
			if(searchTarget()) setState(aiState.MOVE);
		if(currentTime < 10f) return;
		Destroy(gameObject);
		return;
	}

	void moveUpdate(float time) {
		currentTime += time;
		Transform target;
		if(targetUnit == null) {
			if(targetBuilding == null) {
				if(!searchTarget()) setState(aiState.NONE);
				return;
			}
        	target = targetBuilding.gameObject.transform.parent;
		}
		else {
			if(targetUnit == null) {
				if(!searchTarget()) setState(aiState.NONE);
				return;
			}
			target = targetUnit.gameObject.transform;
		}
		Vector3 distance = target.localPosition - transform.localPosition;
        float length = Vector3.Distance(transform.localPosition, target.localPosition);
		setFlip(distance);
		if(isTargetClose(length)) {
			setState(aiState.ATTACK);
			agent.Stop();
			agent.maxSpeed = 0f;
			return;
		}
		agent.SetDestination(target.position);
		if(currentTime < 2f) return;
		currentTime = 0f;
		searchTarget();
	}

	void attackUpdate(float time) {
		currentTime += time;
		if(currentTime < unit.attackSpeed) return;
		currentTime = 0f;
		if(targetUnit != null)
			attackUnit();
		else if(targetBuilding != null)
			attackBuilding();
		else if(targetUnit == null && targetBuilding == null) {
			detectCollider.enabled = true;
			if(searchTarget()) 
				setState(aiState.MOVE);
			else
				setState(aiState.NONE);
		}
		
		spineAnimationState.SetAnimation(0, "attack", false);
		spineAnimationState.AddAnimation(0, "stand", true, 0);
	}

	private void attackBuilding() {
		cityManager.TakeDamage(targetEnum, targetBuilding.tileNum, unit.power);
		if(targetBuilding.hp <= 0) {
            targetBuilding = null;
			if(searchTarget())
				setState(aiState.MOVE);
			else
				setState(aiState.NONE);
		}
	}

	private void attackUnit() {
		targetUnit.damaged(unit.power);
		if(targetUnit.health <= 0f) {
			targetUnit = null;
			detectCollider.enabled = true;
			if(searchTarget())
				setState(aiState.MOVE);
			else
				setState(aiState.NONE);
		}
	}

	private bool isTargetClose(float distance) {
		if(targetBuilding == null && targetUnit == null) {
			searchTarget();
			return false;
		}
		if(distance <= unit.attackRange) 
			return true;
		return false;
	}

	private bool searchTarget() {
		if(protecting) {
			searchUnit();
			return targetUnit != null;
		}
		else {
			searchBuilding();
			return targetBuilding != null;
		}
	}

	private void searchUnit() {
		float distance = 0f;
		UnitAI[] units = transform.parent.GetComponentsInChildren<UnitAI>();
		foreach(UnitAI unit in units) {
			if(unit.gameObject.layer == LayerMask.NameToLayer("PlayerUnit")) continue;
			Vector3 UnitPos = unit.gameObject.transform.position;
			float length = Vector3.Distance(transform.position, UnitPos);
			if(targetUnit == null) {
				targetUnit = unit;
				distance = length;
				continue;
			}
			if(distance > length) {
				targetUnit = unit;
				distance = length;
				continue;
			}
		}
	}

	private void searchBuilding() {
		float distance = 0f;
		foreach(IngameCityManager.BuildingInfo target in buildingInfos) {
			if(target.hp <= 0) continue;
			
			Vector3 buildingPos = target.gameObject.transform.parent.position;
			float length = Vector3.Distance(transform.position, buildingPos);
			if(this.targetBuilding == null) {
				this.targetBuilding = target;
				distance = length;
				continue;
			}
			if(distance > length) {
				this.targetBuilding = target;
				distance = length;
				continue;
			}
		}
	}

	public void damaged(int damage) {
		health -= damage;
		calculateHealthBar();		
	}

	private void calculateHealthBar() {
		if(!healthBar.parent.gameObject.activeSelf) healthBar.parent.gameObject.SetActive(true);
		float percent = (float)health / maxHealth;
		healthBar.transform.localScale = new Vector3(percent, 1f, 1f);
	}

	void setFlip(Vector2 move) {
        skeleton.ScaleX = move.x < 0 ? 1f: -1f;
	}

    public void DestoryEnemy() {
        if (ontile== null) {
            ontile = null;
        }
        else if (ontile.GetComponent<TileCollision>() != null && ontile.GetComponent<TileCollision>().count > 0) {
            ontile.GetComponent<TileCollision>().count--;

            if (ontile.GetComponent<TileCollision>().count <= 0) {
                ontile.GetComponent<TileCollision>().count = 0;
                ontile.GetComponent<TileCollision>().check = false;
            }
        }
        
        Destroy(gameObject);
    
    }
   
	public void NearEnemy(Collider2D other) {
		targetUnit = other.GetComponent<UnitAI>();
		targetBuilding = null;
	}

}
