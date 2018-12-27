using UnityEngine;
using UnityEngine.UI;
using DataModules;

using System.Collections;
using System.Collections.Generic;


public class DeckListController : MonoBehaviour {
    [SerializeField] private GameObject[] slots;

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
        Deck[] decks = _PlayerInfosManager.decks;
        for(int i=0; i<decks.Length; i++) {
            GameObject obj = Instantiate(Modify, slots[i].transform);
            obj.transform.Find("Name").GetComponent<Text>().text = decks[i].Name;
            if (decks[i].isLeader) obj.transform.Find("IsLeader").gameObject.SetActive(true);
        }

        for(int i=decks.Length; i<slots.Length; i++) {
            GameObject obj = Instantiate(Add, slots[i].transform);
        }
    }
}
