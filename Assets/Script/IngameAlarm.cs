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
					Debug.LogWarning("아직 준비가 안됐습니다. 양해바랍니다.");
				}
			}
			return _instance;
		}
	}

	public void Awake() {
		_instance = this;
		gameObject.SetActive(false);
	}

	public void OnDestroy () {
        _instance = null;
    }

	private List<string> alarmList = new List<string>();
	[SerializeField] private GameObject warningEdge;
	[SerializeField] private Text warningText;
	private float colorAlpha;

	enum Category {
		TEXT,
		EDGE,

	}

	private void Update() {
		warningText.color = new Vector4(1f, 1f, 1f, colorAlpha);
		colorAlpha -= 0.005f;
		if(colorAlpha <= 0.01f) gameObject.SetActive(false);
	}

	public void SetAlarm(string text) {
		colorAlpha = 1f;
		warningText.text = text;
		gameObject.SetActive(true);
	}

	public void SetEdgeAlert() {
		SetAlarm("적이 기지를 공격하고 있습니다!");
	}
}
