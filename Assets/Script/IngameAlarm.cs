using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameAlarm : MonoBehaviour {
	private static IngameAlarm _instance;
	public static IngameAlarm instance {
		get {
			if(_instance == null) {
				_instance = (IngameAlarm) FindObjectOfType(typeof(IngameAlarm));
				if(_instance == null) {
					Debug.LogError("인게임 씬이 아닙니다.");
				}
			}
			return _instance;
		}
	}

	public void OnDestroy () {
        _instance = null;
    }

	private List<string> alarmList = new List<string>();
	[SerializeField] private GameObject warningEdge;

	enum Category {
		TEXT,
		EDGE,

	}

	public void SetAlarm(string text) {
		
	}

	public void SetEdgeAlert() {

	}
}
