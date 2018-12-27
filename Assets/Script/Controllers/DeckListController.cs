using UnityEngine;
using UnityEngine.UI;
using DataModules;

using System.Collections;
using System.Collections.Generic;

using UniRx;
using UniRx.Triggers;

public class DeckListController : MonoBehaviour {
    [SerializeField] private GameObject[] slots;
    private List<GameObject> items;
    PlayerInfosManager _PlayerInfosManager;
    public GameObject
        Add,
        Modify;
    // Use this for initialization
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {

    }

    void OnEnable() {
        _PlayerInfosManager = PlayerInfosManager.Instance;
        Initialize();
    }

    private void Initialize() {
        List<Deck> decks = _PlayerInfosManager.decks;
        Sort(decks);
    }

    private void Sort(List<Deck> decks) {
        items = new List<GameObject>();
        Clear();
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
                });
            items.Add(newItem);
        }
        for (int i = decks.Count; i < slots.Length; i++) {
            GameObject newItem = Instantiate(Add, slots[i].transform);
            items.Add(newItem);
        }
    }

    private void Clear() {
        foreach(GameObject slot in slots) {
            foreach(Transform tf in slot.transform) {
                Destroy(tf.gameObject);
            }
        }
    }
}
