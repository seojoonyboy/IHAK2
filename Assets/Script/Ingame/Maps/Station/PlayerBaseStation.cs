using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseStation : DefaultStation {
    

	// Use this for initialization
	void Start () {
        SettingFog();

        if (gameObject.name == "S10")
            OwnerNum = PlayerController.Player.PLAYER_1;
        else
            OwnerNum = PlayerController.Player.PLAYER_2;

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
