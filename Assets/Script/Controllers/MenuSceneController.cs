using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class MenuSceneController : MonoBehaviour {

    public enum Windows {
        BASIC = 0,
        DECKLIST = 1,
        SHOP = 2
    }

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.MenuScene;

    [SerializeField] Transform buttonSelect;
    [SerializeField] Transform buttonList;
    [SerializeField] GameObject switchButtons;
    [SerializeField] Text userNickname;
    [SerializeField] GameObject leaderDeck;

    private HorizontalScrollSnap hss;
    private Windows openedWindow;
    private static int pageNum = 1;

    MenuSceneEventHandler eventHandler;

    private void Awake() {
        eventHandler = MenuSceneEventHandler.Instance;
        eventHandler.AddListener(MenuSceneEventHandler.EVENT_TYPE.CHANGE_MAINSCENE_TILE_GROUP, ResetTileGroupFinished);

        eventHandler.PostNotification(MenuSceneEventHandler.EVENT_TYPE.CHANGE_MAINSCENE_TILE_GROUP, null, AccountManager.Instance.leaderIndex);
    }

    private void OnDestroy() {
        eventHandler.RemoveListener(MenuSceneEventHandler.EVENT_TYPE.CHANGE_MAINSCENE_TILE_GROUP, ResetTileGroupFinished);
    }

    private void ResetTileGroupFinished(Enum Event_Type, Component Sender, object Param) {
        foreach(Transform tf in leaderDeck.transform) {
            Destroy(tf.gameObject);
        }
        GameObject go = AccountManager.Instance.transform.GetChild(0).GetChild((int)Param).gameObject;
        go.SetActive(true);
        SetProdPowerInfo(ref go);
        GameObject lo = Instantiate(go, leaderDeck.transform);

        foreach(Transform tile in lo.transform) {
            if(tile.childCount > 1) {
                for(int i= 1; i<tile.childCount; i++) {
                    Destroy(tile.GetChild(i).gameObject);
                }
            }
        }
        go.SetActive(false);
    }

    //Server에 생산력 총합에 대한 Data가 없어 Client에서 해당 처리를 진행
    //Server구축시 재변경
    private void SetProdPowerInfo(ref GameObject go) {
        int env = 0;
        int gold = 0;
        int food = 0;

        foreach (Transform tile in go.transform) {
            if(tile.childCount >= 1) {
                DataModules.Card card = tile.GetChild(0).GetComponent<BuildingObject>().data.card;
                string type = card.prodType;
                DataModules.Cost product = card.product;

                if (type == "food") food += product.food;
                else if (type == "gold") gold += product.gold;
                else if (type == "env") env += product.environment;
            }
        }
        DataModules.Resources resources = new DataModules.Resources();
        resources.env = env;
        resources.food = food;
        resources.gold = gold;
        go.GetComponent<TileGroup>().touchPerProdPower = resources;
    }

    // Use this for initialization
    void Start() {
        openedWindow = Windows.BASIC;
        hss = FindObjectOfType<HorizontalScrollSnap>();
        userNickname.text = AccountManager.Instance.userInfos.nickname;
        clickMenuButton(pageNum);
        //MenuSceneEventHandler.Instance.PostNotification(MenuSceneEventHandler.EVENT_TYPE.SET_TILE_OBJECTS_COMPLETED, null);
    }

    public void switchButton() {
        iTween.MoveTo(buttonSelect.GetChild(1).gameObject, iTween.Hash("x", buttonList.GetChild(hss.CurrentPage).position.x, "time", 0.3f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
        pageNum = hss.CurrentPage;
        StartCoroutine(HideButton());
    }

    IEnumerator HideButton() {
        yield return new WaitForSeconds(0.2f);
        switch (pageNum) {
            case 0:
                buttonSelect.GetChild(1).GetChild(0).gameObject.SetActive(false); // 왼쪽 화살표 (MenuCanvas/MainButtons/SelectedButton/ -> 0번째 자식)
                buttonSelect.GetChild(1).GetChild(1).gameObject.SetActive(true); // 왼쪽 화살표 (MenuCanvas/MainButtons/SelectedButton/ -> 1번째 자식)
                break;
            
            case 2:
                buttonSelect.GetChild(1).GetChild(0).gameObject.SetActive(true);
                buttonSelect.GetChild(1).GetChild(1).gameObject.SetActive(false);
                break;
            default :
                buttonSelect.GetChild(1).GetChild(0).gameObject.SetActive(true);
                buttonSelect.GetChild(1).GetChild(1).gameObject.SetActive(true);
                break;
        }
    }

    public void clickMenuButton(int page) {
        hss.GoToScreen(page);
        switchButton();
    }

    public void OpenOption() {
        OptionController oc = FindObjectOfType<OptionController>();
        oc.EnterOptionWindow(true);
    }
}
