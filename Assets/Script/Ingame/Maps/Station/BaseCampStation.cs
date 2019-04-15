using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BaseCampStation : DefaultStation {

    List<GameObject> creepList;

	// Use this for initialization
	void Start () {
        PlayerNum = PlayerController.Player.NEUTRAL;
        creepList = new List<GameObject>();
    }
	
	// Update is called once per frame
	void Update () {
		if(creepList.Count == 0) {

        }
	}
}
