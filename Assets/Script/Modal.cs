using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Modal : MonoBehaviour {
	[SerializeField] private Button yesButton;
	[SerializeField] private Button noButton;
	[SerializeField] private Text describe;

	public enum Type {
		YESNO,
		CHECK,
		INSERT
	}
	/// <summary>
	/// Modal 창 생성기
	/// </summary>
	/// <param name="text">설명에 들어갈 내용</param>
	/// <param name="type">Modal.Type.종류</param>
	/// <param name="function">yes 버튼 누를 경우 실행 함수(필요하면)</param>
	public static void instantiate(string text, Type type, UnityAction function = null) {
		GameObject modal = Resources.Load("Prefabs/ModalWindow", typeof(GameObject)) as GameObject;
        Canvas canvas = (Canvas)FindObjectOfType(typeof(Canvas));
        if(canvas == null) {
            Debug.LogError("no Canvas");
            return;
        }
        Instantiate(modal, canvas.transform, false).GetComponent<Modal>().setData(text, function, type);
	}

	public void setData(string text, UnityAction function, Type type) {
		if(type == Type.CHECK) {
			noButton.gameObject.SetActive(false);
			yesButton.GetComponentInChildren<Text>().text = "확인";
		}
		describe.text = text;
		if(function == null) return;
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
