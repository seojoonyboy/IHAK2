using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDetector : MonoBehaviour {
	private UnitAI unitAI;
	[HideInInspector] public int detectingLayer;
	private void Start() {
		unitAI = GetComponentInParent<UnitAI>();
	}

	private void OnTriggerStay2D(Collider2D other) {
		if(other.gameObject.layer == detectingLayer) {
			if(other.GetComponent<UnitAI>() == null) return;
			unitAI.NearEnemy(other);
			GetComponent<CircleCollider2D>().enabled = false;
		}
	}
}
