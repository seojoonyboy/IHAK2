using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class DeckEditor : MonoBehaviour {

    public GameObject tileGroup;
    public GameObject editSceneUI;    
    AccountManager userAccount;
    public List<int> deckData;
    public Button resetButton;

	void Start () {
        userAccount = AccountManager.Instance;

        userAccount.transform.GetChild(0).GetChild(userAccount.selectNumber).gameObject.SetActive(true);
        tileGroup = userAccount.transform.GetChild(0).GetChild(userAccount.selectNumber).gameObject;

        for (int i = 0; i < tileGroup.transform.childCount; i++)
            deckData.Add(0);

        resetButton.OnClickAsObservable().Subscribe(_ =>resetTile());


    }
    
    public void resetTile() {
        for(int i = 0; i<tileGroup.transform.childCount; i++) {
            Destroy(tileGroup.transform.GetChild(0));
            GetComponent<TileObject>().buildingSet = false;
        }
    }


}
