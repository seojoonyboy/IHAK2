using Container;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayerController : MonoBehaviour {
    [SerializeField] Transform tileGroupParent;
    [SerializeField] GameObject dummyPref;

    [SerializeField] [ReadOnly] DeckInfo deckInfo;
    [SerializeField] [ReadOnly] PlayerResource playerResource;
    public GameObject goblin;    

    void Start() {
        deckInfo = Instantiate(dummyPref, tileGroupParent)
            .transform
            .GetComponent<DeckInfo>();

        //deckInfo.transform.Find("Background").gameObject.SetActive(false);
        playerResource = GetComponent<PlayerResource>();

        GetComponent<EnemyBuildings>().Init();
    }
}
