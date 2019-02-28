using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;

public class EditScenePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.DeckSettingScene;

    [SerializeField] Transform content;
    [SerializeField] Transform leftBtn;
    [SerializeField] Transform rightBtn;
    [SerializeField] Text pageText;
    public int page;
    public int maxPage;
    public bool pointerInside = false;

    public DeckSettingController deckSettingController;
    private float mouseDownPosition;

    public List<Vector3> pageLocation;

    private void Start() {
        page = 0;
        deckSettingController = transform.parent.GetComponent<DeckSettingController>();

        leftBtn.GetComponent<Button>().OnClickAsObservable().ThrottleFirst(TimeSpan.FromMilliseconds(500)).Subscribe(_ => switchButton(true));
        rightBtn.GetComponent<Button>().OnClickAsObservable().ThrottleFirst(TimeSpan.FromMilliseconds(500)).Subscribe(_ => switchButton(false));

        var downStream = content.gameObject.UpdateAsObservable().Where(_ => Input.GetMouseButtonDown(0)).Select(_ => mouseDownPosition = Input.mousePosition.x);
        var upStream = content.gameObject.UpdateAsObservable().Where(_ => Input.GetMouseButtonUp(0)).Select(_ => Input.mousePosition.x);
        var dragStream = content.gameObject.UpdateAsObservable().SkipUntil(downStream).TakeUntil(upStream).RepeatUntilDestroy(this);

        dragStream.Where(_ => mouseDownPosition - Input.mousePosition.x < -Screen.width/5.0f).ThrottleFirst(TimeSpan.FromMilliseconds(250)).Subscribe(_ => switchButton(true));
        dragStream.Where(_ => mouseDownPosition - Input.mousePosition.x > Screen.width/5.0f).ThrottleFirst(TimeSpan.FromMilliseconds(250)).Subscribe(_ => switchButton(false));

        leftBtn.gameObject.SetActive(false);
    }

    public void switchButton(bool left) {
        if (deckSettingController.picking == false) {
            if (left) {
                if (page == 0)
                    return;

                page--;

                for (int i = 0; i < maxPage; i++)
                    iTween.MoveTo(content.GetChild(i).gameObject, iTween.Hash(
                        "x", pageLocation[maxPage - page - 1 + i].x,
                        "time", 0.4f,
                        "delay", 0,
                        "easetype", iTween.EaseType.easeInOutQuart));
                //"onstarttarget", gameObject,
                //"onstart", "DisableArrowButtons",
                //"oncompletetarget", gameObject,
                //"oncomplete", "EnableArrowButtons"));


            }
            else {
                if (page == maxPage - 1)
                    return;

                page++;

                for (int i = 0; i < maxPage; i++)
                    iTween.MoveTo(content.GetChild(i).gameObject, iTween.Hash("x", pageLocation[maxPage - page - 1 + i].x,
                    "time", 0.4f,
                    "delay", 0,
                    "easetype", iTween.EaseType.easeInOutQuart));
                //"onstarttarget", gameObject,
                //"onstart", "DisableArrowButtons",
                //"oncompletetarget", gameObject,
                //"oncomplete", "EnableArrowButtons"));

            }
            pageText.text = (page + 1) + " / " + content.childCount;

            if (page == 0) {
                leftBtn.gameObject.SetActive(false);
                rightBtn.gameObject.SetActive(true);
            }
            else if (page == maxPage - 1) {
                leftBtn.gameObject.SetActive(true);
                rightBtn.gameObject.SetActive(false);
            }
            else {
                leftBtn.gameObject.SetActive(true);
                rightBtn.gameObject.SetActive(true);
            }
        }
    }

    private void DisableArrowButtons() {
        leftBtn.GetComponent<Button>().interactable = false;
        rightBtn.GetComponent<Button>().interactable = false;
    }

    private void EnableArrowButtons() {
        leftBtn.GetComponent<Button>().interactable = true;
        rightBtn.GetComponent<Button>().interactable = true;
    }

    public void SavePagePosition() {
        Vector3 page;

        for(int i = 1 - maxPage ; i < maxPage; i++) {
            page = new Vector3(content.GetChild(0).position.x + (Screen.width * i),0,0);
            pageLocation.Add(page);           
        }
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
        pointerInside = true;
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
        pointerInside = false;
    }

}
