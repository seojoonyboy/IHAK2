using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UniRx;

public partial class DefaultStation : SerializedMonoBehaviour {

    [SerializeField] public GameObject fogLight; 

    public void SettingFog() {
        LoadFogLight();
        PostObservableOwner();
    }

    // Use this for initialization
    public void LoadFogLight() {
        GameObject view = Instantiate(ConstructManager.Instance.unfog, transform);
        view.transform.SetAsLastSibling();
        view.SetActive(false);
        fogLight = view;
    }

    public void EraseFog() {
        if (fogLight == null) return;
        fogLight.SetActive(true);
    }

    public void ReleaseFog() {
        if (fogLight == null) return;
        fogLight.SetActive(false);
    }

    public void PostObservableOwner() {
        Observable.EveryUpdate().Select(_ => ownerNum).DistinctUntilChanged().Where(x => x == PlayerController.Player.PLAYER_1).Subscribe(_ => EraseFog()).AddTo(this);
        Observable.EveryUpdate().Select(_ => ownerNum).DistinctUntilChanged().Where(x => x != PlayerController.Player.PLAYER_1).Subscribe(_ => ReleaseFog()).AddTo(this);
    }

}

public partial class DefaultStation : SerializedMonoBehaviour {    
    public UniRx.ReactiveProperty<int> intervalTime;
    public int pivotTime;
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
                try {
                    IngameAlarm.instance.SetAlarm(ownerNum + "가 거점을 점령하였습니다.");
                }
                catch (System.NullReferenceException ne) {
                    Debug.Log("초기세팅");
                }
            }
        }
    }

    public StationBasic.StationState StationIdentity {
        set { stationIdentity = value; }
    }

    public GameObject Building{
        get { return building; } set { building = value; }
    }

    public virtual void DestroyEnteredTarget(GameObject unitObj) {}
}
