using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Modal : MonoBehaviour {
	[SerializeField] private Button yesButton;
	[SerializeField] private Button noButton;
	[SerializeField] private Text describe;
	[SerializeField] private GameObject inputGameObject;
	[SerializeField] private Text insertTitle;
	[SerializeField] private InputField inputField;

	public enum Type {
		YESNO,
		CHECK,
		INSERT
	}
	/// <summary>
	/// Modal 창 생성기 (YESNO와 CHECK 편)
	/// </summary>
	/// <param name="text">설명에 들어갈 내용</param>
	/// <param name="type">Modal.Type.종류</param>
	/// <param name="function">yes 버튼 누를 경우 실행 함수(필요하면)</param>
	public static void instantiate(string text, Type type, UnityAction function = null) {
		if(type == Type.INSERT) {
			Debug.LogWarning("enum INSERT는 매개변수 하나 더 있습니다!");
			return;
		}
		GameObject modal = Resources.Load("Prefabs/ModalCanvas", typeof(GameObject)) as GameObject;
        Canvas canvas = (Canvas)FindObjectOfType(typeof(Canvas));
        if(canvas == null) {
            Debug.LogError("no Canvas");
            return;
        }
        Instantiate(modal).GetComponent<Modal>().setData(text, function, type);
        //Instantiate(modal, canvas.transform, false).GetComponent<Modal>().setData(text, function, type);
	}
	/// <summary>
	/// Modal 창 생성기 (Insert 편) 
	/// </summary>
	/// <param name="text">제목 들어갈 내용</param>
	/// <param name="inputText">inputField 빈공간에 들어갈 내용</param>
	/// <param name="type">Modal.Type.종류</param>
	/// <param name="function">yes 버튼 누를 경우 실행 함수</param>
	public static void instantiate(string text, string inputText, Type type, UnityAction<string> function) {
		if(type != Type.INSERT) {
			Debug.LogWarning("enum YESNO 또는 CHECK는 매개변수를 줄여주십시오!");
			return;
		}
		GameObject modal = Resources.Load("Prefabs/ModalCanvas", typeof(GameObject)) as GameObject;
        Canvas canvas = (Canvas)FindObjectOfType(typeof(Canvas));
        if(canvas == null) {
            Debug.LogError("no Canvas");
            return;
        }
        Instantiate(modal).GetComponent<Modal>().setData(text, inputText, function);
        //Instantiate(modal, canvas.transform, false).GetComponent<Modal>().setData(text, inputText, function);
	}

	public void setData(string text, UnityAction function, Type type) {
		if(type == Type.CHECK) {
			noButton.gameObject.SetActive(false);
			yesButton.GetComponentInChildren<Text>().text = "확인";
		}
		describe.text = text;
		yesButton.onClick.AddListener(closeButton);
		if(function == null) return;
		yesButton.onClick.AddListener(function);
	}

	public void setData(string text, string inputText, UnityAction<string> function) {
		describe.gameObject.SetActive(false);
		inputGameObject.SetActive(true);
		noButton.gameObject.SetActive(false);
		yesButton.GetComponentInChildren<Text>().text = "저장";
		insertTitle.text = text;
		inputField.GetComponentInChildren<Text>().text = inputText;
		yesButton.onClick.AddListener(() => {checkInputText(inputField.text, function);});
	}

	private void checkInputText(string text, UnityAction<string> function) {
		if(string.IsNullOrEmpty(text)) {
			instantiate("내용이 비어있습니다\n내용을 채워주세요", Type.CHECK);
			return;
		}
		function(text);
		closeButton();
	}

	private void Start() {
		noButton.onClick.AddListener(closeButton);
	}

	private void closeButton() {
		Destroy(gameObject);
	}
}
