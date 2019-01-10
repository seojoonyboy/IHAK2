using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class IngameSceneUIController : MonoBehaviour {

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.IngameScene;

    [SerializeField] Transform territoryList;
    [SerializeField] Transform camera;

    private float mousDownPosition;

	// Use this for initialization
	void Start () {
        var downStream = this.gameObject.UpdateAsObservable().Where(_ => Input.GetMouseButtonDown(0)).Select(_ => mousDownPosition = Input.mousePosition.x);
        var upStream = this.gameObject.UpdateAsObservable().Where(_ => Input.GetMouseButtonUp(0)).Select(_ => Input.mousePosition.x);
        var dragStream = this.gameObject.UpdateAsObservable().SkipUntil(downStream).TakeUntil(upStream).RepeatUntilDestroy(this);
        dragStream.Where(_ => mousDownPosition - Input.mousePosition.x < -500).ThrottleFirst(TimeSpan.FromMilliseconds(450)).Subscribe(_ => StartCoroutine(SwitchTerritory(true)));
        dragStream.Where(_ => mousDownPosition - Input.mousePosition.x > 500).ThrottleFirst(TimeSpan.FromMilliseconds(450)).Subscribe(_ => StartCoroutine(SwitchTerritory(false)));
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator SwitchTerritory(bool left) {
        if (left && territoryList.localPosition.x < -1080) {
            iTween.MoveTo(territoryList.gameObject, iTween.Hash("x", territoryList.position.x + Screen.width, "time", 0.4f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
            yield return new WaitForSeconds(0.38f);
            this.transform.GetChild(3).gameObject.SetActive(false);
            this.transform.GetChild(2).gameObject.SetActive(true);
        }
        else if (!left && territoryList.localPosition.x > -1) {
            iTween.MoveTo(territoryList.gameObject, iTween.Hash("x", territoryList.position.x - Screen.width, "time", 0.4f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
            yield return new WaitForSeconds(0.38f);
            this.transform.GetChild(2).gameObject.SetActive(false);
            this.transform.GetChild(3).gameObject.SetActive(true);
        }
        
    }
}
