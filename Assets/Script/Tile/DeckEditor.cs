using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class DeckEditor : MonoBehaviour {

    public GameObject tileGroup;
    public GameObject editSceneUI;    
    AccountManager userAccount;
    DropHandler dropHandler;

	void Start () {
        userAccount = AccountManager.Instance;
        dropHandler = editSceneUI.transform.GetChild(0).GetChild(0).GetComponent<DropHandler>();

        userAccount.transform.GetChild(0).GetChild(3).gameObject.SetActive(true);
        tileGroup = userAccount.transform.GetChild(0).GetChild(3).gameObject;
        dropHandler.tileGroup = tileGroup;

        for (int i = 0; i < tileGroup.transform.childCount; i++)
            dropHandler.deckData.Add(0);



    }	
}
