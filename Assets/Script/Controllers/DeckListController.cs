using UnityEngine;
using UnityEngine.UI;
using DataModules;

using System.Collections;
using System.Collections.Generic;

using UniRx;
using UniRx.Triggers;

public class DeckListController : MonoBehaviour {

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.MenuScene;

    [SerializeField] private GameObject[] slots;
    private List<GameObject> items;
    PlayerInfosManager _PlayerInfosManager;
    public GameObject
        Add,
        Modify;

    void OnEnable() {
        _PlayerInfosManager = PlayerInfosManager.Instance;
        Initialize();
    }

    private void Initialize() {
        List<Deck> decks = _PlayerInfosManager.decks;
        Sort(decks);
    }

    private void Sort(List<Deck> decks) {
        Clear();

        items = new List<GameObject>();
        //id 기준으로 정렬, 대표 덱을 맨 앞으로 정렬
        decks.Sort((a, b) => a.Id.CompareTo(b.Id));
        Deck deck = decks.Find(x => x.isLeader == true);
        if (decks.Count > 1 && deck != null) {
            decks.Remove(deck);
            decks.Insert(0, deck);
        }

        for (int i = 0; i < decks.Count; i++) {
            GameObject newItem = Instantiate(Modify, slots[i].transform);
            newItem.transform.Find("Name").GetComponent<Text>().text = decks[i].Name;
            newItem.transform.Find("IsLeader").gameObject.SetActive(decks[i].isLeader);
            int id = decks[i].Id;
            newItem.transform.Find("DeleteBtn").GetComponent<Button>().onClick
                .AsObservable()
                .Subscribe(_ => {
                    _PlayerInfosManager.RemoveDeck(id);
                    Sort(_PlayerInfosManager.decks);
                    //ReassignLeaderDeck();
                });
            items.Add(newItem);
        }
        for (int i = decks.Count; i < slots.Length; i++) {
            GameObject newItem = Instantiate(Add, slots[i].transform);
            items.Add(newItem);
            newItem.GetComponent<Button>().onClick.AsObservable().Subscribe(_ => {
                moveToDeckSetting();
            });
        }
    }

    private void Clear() {
        foreach (GameObject slot in slots) {
            foreach (Transform tf in slot.transform) {
                Destroy(tf.gameObject);
            }
        }
    }
    private void ReassignLeaderDeck() {

    }

    public void moveToDeckSetting() {
        GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
        gsm.startScene(sceneState.ToString(), GameSceneManager.SceneState.DeckSettingScene);
    }
}
