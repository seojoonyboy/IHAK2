using UnityEngine;
using UnityEngine.UI;
using DataModules;

using System.Collections;
using System.Collections.Generic;

using UniRx;
using UniRx.Triggers;
using System;

public class DeckListController : MonoBehaviour {

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.MenuScene;
    MenuSceneEventHandler eventHandler;

    [SerializeField] private GameObject[] slots;
    private List<GameObject> items;
    public GameObject
        Add,
        Modify;

    public GameObject temp;

    void Start() {
        AccountManager.Instance.GetMyDecks();

        eventHandler = MenuSceneEventHandler.Instance;
        eventHandler.RemoveListener(MenuSceneEventHandler.EVENT_TYPE.DECKLIST_CHANGED, OnDeckChanged);
        eventHandler.AddListener(MenuSceneEventHandler.EVENT_TYPE.DECKLIST_CHANGED, OnDeckChanged);
    }

    private void OnDeckChanged(Enum Event_Type, Component Sender, object Param) {
        Initialize();
    }

    public void Initialize() {
        List<Deck> decks = AccountManager.Instance.decks;
        Sort(decks);
    }

    private void Sort(List<Deck> decks) {
        Clear();

        items = new List<GameObject>();

        for (int i = 0; i < slots.Length; i++) {
            if (slots[i] == null || decks.Count == 0 || decks.Count - 1 < i) break;
            Debug.Log(i);
            GameObject newItem = Instantiate(Modify, slots[i].transform);
            newItem.transform.Find("Name").GetComponent<Text>().text = decks[i].name;

            if(i == 0) newItem.transform.Find("LeaderSetBtn/IsLeader").gameObject.SetActive(true);
            else newItem.transform.Find("LeaderSetBtn/IsLeader").gameObject.SetActive(false);

            int id = decks[i].id;
            newItem.GetComponent<Index>().Id = id;
            newItem.transform.Find("DeleteBtn").GetComponent<Button>().onClick
                .AsObservable()
                .Subscribe(_ => {
                    Modal.instantiate((AccountManager.Instance.FindDeck(id)).name + "덱을 삭제하겠습니까?", Modal.Type.YESNO, () => {
                        AccountManager.Instance.RemoveDeck(id);
                        Sort(AccountManager.Instance.decks);
                        AccountManager.Instance.RemoveTileObjects(newItem.transform.parent.GetSiblingIndex());
                    });
                });
            newItem.transform.Find("LeaderSetBtn").GetComponent<Button>().onClick
                .AsObservable()
                .Subscribe(_ => {
                    Modal.instantiate((AccountManager.Instance.FindDeck(id)).name + "덱을 대표 덱으로\n설정하시겠습니까?", Modal.Type.YESNO, () => {
                        AccountManager.Instance.ChangeLeaderDeck(id);
                        Sort(AccountManager.Instance.decks);
                    });
                });
            newItem.transform.Find("ModifyBtn").GetComponent<Button>().onClick
                .AsObservable()
                .Subscribe(_ => {
                    moveToDeckSetting(decks.Find(x => x.id == id));
                    AccountManager.Instance.selectNumber = newItem.transform.parent.GetComponent<Index>().Id;
                    //Debug.Log(newItem.transform.parent.GetComponent<Index>().Id);
                });
            items.Add(newItem);
        }
        for (int i = decks.Count; i < slots.Length; i++) {
            if (slots[i] == null) break;
            GameObject newItem = Instantiate(Add, slots[i].transform);
            items.Add(newItem);
            newItem.GetComponent<Button>().onClick.AsObservable().Subscribe(_ => {
                //AccountManager.Instance.selectNumber = newItem.transform.parent.GetSiblingIndex();
                AccountManager.Instance.selectNumber = newItem.transform.parent.GetComponent<Index>().Id;
                moveToDeckSetting();
            });
        }
    }

    private void Clear() {
        foreach (GameObject slot in slots) {
            if (slot == null) break;
            foreach (Transform tf in slot.transform) {
                Destroy(tf.gameObject);
            }
        }
    }

    public void moveToDeckSetting(Deck building = null) {
        GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
        gsm.startScene(sceneState, GameSceneManager.SceneState.DeckSettingScene);
        DeckSettingController.prevData = building;
    }
}
