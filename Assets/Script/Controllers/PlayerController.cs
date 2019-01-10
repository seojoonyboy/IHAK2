using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    public enum Buttons {
        GOLD = 0,
        FOOD = 1,
        ENVIRONMENT = 2,
        REPAIR = 3,

    }

    private GameSceneManager.SceneState sceneState = GameSceneManager.SceneState.IngameScene;


    [SerializeField] Transform commandButtons;
    [SerializeField] Transform playerResource;

    private int gameTurn;

	// Use this for initialization
	void Start () {
        gameTurn = 600;
        playerResource.Find("Turn/Value").gameObject.GetComponent<Text>().text = gameTurn.ToString();

        commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => gameTurn > 0).Subscribe(_ => ClickButton(Buttons.GOLD));
        commandButtons.GetChild(1).GetComponent<Button>().OnClickAsObservable().Where(_ => gameTurn > 0).Subscribe(_ => ClickButton(Buttons.FOOD));
        commandButtons.GetChild(2).GetComponent<Button>().OnClickAsObservable().Where(_ => gameTurn > 0).Subscribe(_ => ClickButton(Buttons.ENVIRONMENT));
        //commandButtons.GetChild(0).GetComponent<Button>().OnClickAsObservable().Where(_ => gameTurn > 0).Subscribe(_ => ClickButton(Buttons.REPAIR));

    }

    private void ClickButton(Buttons btn) {
        switch (btn){
            case Buttons.GOLD:
                gameTurn--;
                break;
            case Buttons.FOOD:
                gameTurn--;
                break;
            case Buttons.ENVIRONMENT:
                gameTurn--;
                break;
            case Buttons.REPAIR:
                gameTurn--;
                break;
        }
        playerResource.Find("Turn/Value").gameObject.GetComponent<Text>().text = gameTurn.ToString();
    }
}
