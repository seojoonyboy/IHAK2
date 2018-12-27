using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Modal : MonoBehaviour {
	[SerializeField] private Button yesButton;
	[SerializeField] private Button noButton;
	[SerializeField] private Text describe;

	public static void instantiate(string text, UnityAction function) {
		GameObject modal = Resources.Load("Prefabs/ModalWindow", typeof(GameObject)) as GameObject;
        Canvas canvas = (Canvas)FindObjectOfType(typeof(Canvas));
        if(canvas == null) {
            Debug.LogError("no Canvas");
            return;
        }
        Instantiate(modal, canvas.transform, false).GetComponent<Modal>().setData(text, function);
	}

	public void setData(string text, UnityAction function) {
		describe.text = text;
		yesButton.onClick.AddListener(function);
	}

	private void Start() {
		yesButton.onClick.AddListener(closeButton);
		noButton.onClick.AddListener(closeButton);
	}

	private void closeButton() {
		Destroy(gameObject);
	}
}
