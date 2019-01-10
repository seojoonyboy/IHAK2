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
    public GameObject
        Add,
        Modify;

    void OnEnable() {
        Initialize();
    }

    public void Initialize() {
        List<Deck> decks = AccountManager.Instance.decks;
        Sort(decks);
    }

    private void Sort(List<Deck> decks) {
        Clear();

        items = new List<GameObject>();

        for (int i = 0; i < decks.Count; i++) {
            GameObject newItem = Instantiate(Modify, slots[i].transform);
            newItem.transform.Find("Name").GetComponent<Text>().text = decks[i].Name;
            newItem.transform.Find("LeaderSetBtn/IsLeader").gameObject.SetActive(decks[i].isLeader);
            int id = decks[i].Id;
            newItem.GetComponent<Index>().Id = id;
            newItem.transform.Find("DeleteBtn").GetComponent<Button>().onClick
                .AsObservable()
                .Subscribe(_ => {
                    Modal.instantiate((AccountManager.Instance.FindDeck(id)).Name + "덱을 삭제하겠습니까?", Modal.Type.YESNO, () => {
                        AccountManager.Instance.RemoveDeck(id);
                        Sort(AccountManager.Instance.decks);
                    });
                });
            newItem.transform.Find("LeaderSetBtn").GetComponent<Button>().onClick
                .AsObservable()
                .Subscribe(_ => {
                    Modal.instantiate((AccountManager.Instance.FindDeck(id)).Name + "덱을 대표 덱으로\n설정하시겠습니까?", Modal.Type.YESNO, () => {
                        AccountManager.Instance.ChangeLeaderDeck(id);
                        Sort(AccountManager.Instance.decks);
                    });
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

    public void moveToDeckSetting() {
        GameSceneManager gsm = FindObjectOfType<GameSceneManager>();
        gsm.startScene(sceneState, GameSceneManager.SceneState.DeckSettingScene);
    }
}
