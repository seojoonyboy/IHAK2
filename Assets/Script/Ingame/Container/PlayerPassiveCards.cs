using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPassiveCards : SerializedMonoBehaviour {
    public List<GameObject> passiveCards;
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<Passives, GameObject> effectModules;

    [SerializeField] [ReadOnly] PlayerController playerController;
    [SerializeField] [ReadOnly] TileGroup tileGroup;

    public void Init() {
        playerController = PlayerController.Instance;
        try {
            tileGroup =
            playerController.maps[PlayerController.Player.PLAYER_1]
            .transform.GetChild(0)
            .gameObject
            .GetComponent<TileGroup>();
        }
        catch (NullReferenceException ex) {
            Debug.LogError("TileGroup을 찾을 수 없습니다.");
            return;
        }


    }



    public enum Passives {
        mysterious_mine,
        primal_square,
        natural_scanvenger
    }
}
