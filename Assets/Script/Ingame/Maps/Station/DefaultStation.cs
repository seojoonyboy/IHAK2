using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public partial class DefaultStation : SerializedMonoBehaviour {

    
	// Use this for initialization
	void Start () {
    }
}

public partial class DefaultStation : SerializedMonoBehaviour {
    [Header(" - Owned Player ")]
    [SerializeField] PlayerController.Player playerNum;

    [Header(" - Station Identity")]
    [SerializeField] StationBasic.StationState stationIdentity;

    public PlayerController.Player PlayerNum {
        set { playerNum = value; }
    }

    public StationBasic.StationState StationIdentity {
        set { stationIdentity = value; }
    }
}
