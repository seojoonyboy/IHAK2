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
    [SerializeField] PlayerController.Player ownerNum;

    [Header(" - Station Identity")]
    [SerializeField] StationBasic.StationState stationIdentity;

    [Header(" - Ingame Building Info")]
    [SerializeField] GameObject building;

    public PlayerController.Player OwnerNum {
        get { return ownerNum; }
        set {
            ownerNum = value;
            if(ownerNum == PlayerController.Player.PLAYER_1) {
                IngameAlarm.instance.SetAlarm(ownerNum + "가 거점을 점령하였습니다.");
            }
        }
    }

    public StationBasic.StationState StationIdentity {
        set { stationIdentity = value; }
    }

    public GameObject Building{
        get { return building; } set { building = value; }
    }
}
