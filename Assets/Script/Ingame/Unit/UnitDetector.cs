using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDetector : MonoBehaviour {
	private UnitAI unitAI;
	private int detectingLayer;
	private void Start() {
		unitAI = GetComponentInParent<UnitAI>();
	}

	public void SetData(float attackRange, int enemyLayer) {
		gameObject.layer = transform.parent.gameObject.layer;
		CircleCollider2D detectCollider = transform.GetComponent<CircleCollider2D>();
        detectCollider.radius = attackRange;
        detectingLayer = enemyLayer;
	}

	private void OnTriggerStay2D(Collider2D other) {
		int otherLayer = (1 << other.gameObject.layer);
		if((detectingLayer & otherLayer) == otherLayer) {		
			if(other.GetComponent<AI.SkyNet>() == null) return;
			unitAI.NearEnemy(other);
				GetComponent<CircleCollider2D>().enabled = false;
		}
	}

	public void EnemyDone() {
		GetComponent<CircleCollider2D>().enabled = true;
	}
}
