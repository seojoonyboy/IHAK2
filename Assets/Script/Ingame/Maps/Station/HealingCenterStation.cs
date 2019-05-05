using ingameUIModules;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class HealingCenterStation : DefaultStation {

    [SerializeField] [ReadOnly] protected bool startSeize = false;
    [SerializeField] [ReadOnly] protected int seizePlayer;
    [SerializeField] [ReadOnly] GameObject occupySlider;
    [SerializeField] [ReadOnly] CircularSlider circularSlider;

    bool isAlreadyCoroutine = false;
    IEnumerator coroutine;
    // Use this for initialization
    void Start () {
        OwnerNum = PlayerController.Player.NEUTRAL;
        StationIdentity = StationBasic.StationState.HealingCenter;
        enemys = new List<GameObject>();
        healingTarget = new List<GameObject>();
        Building = Resources.Load("Prefabs/FieldHospital") as GameObject;
        Instantiate(Building, transform);
        SettingFog();

        occupySlider = Instantiate(Resources.Load("Prefabs/OccupySlider") as GameObject, transform);
        circularSlider = occupySlider.GetComponent<CircularSlider>();
    }

    private void Update() {
        //if(OwnerNum == PlayerController.Player.NEUTRAL && !startSeize && enemys.Count > 0) {
        //    startSeize = true;
        //    seizePlayer = enemys[0].layer;
        //    StartCoroutine(SeizeNeutralBuilding());
        //}

        //점령한 유닛이 없고 상대 유닛이 왔을 때 점령 시작
        //Coroutine이 2번 시작되는 버그가 있음
        if (!startSeize && enemys.Count > 0 && healingTarget.Count == 0) {
            startSeize = true;
            coroutine = SeizeBuilding();
            StartCoroutine(coroutine);
        }

        if (enemys.Count == 0) {
            if(coroutine != null) {
                isAlreadyCoroutine = false;
                StopCoroutine(coroutine);
                circularSlider.Reset();
            }
            startSeize = false;
        }
    }

    public override void DestroyEnteredTarget(GameObject unitObj) {
        if (enemys.Contains(unitObj)) enemys.Remove(unitObj);
        if (healingTarget.Contains(unitObj)) healingTarget.Remove(unitObj);
    }

    IEnumerator SeizeBuilding() {
        if (isAlreadyCoroutine) StopCoroutine(coroutine);

        isAlreadyCoroutine = true;
        int time = 0;
        circularSlider.Reset();

        while (startSeize) {
            if (enemys.Count == 0) {
                startSeize = false;
                break;
            }

            if (canSeize()) {
                time++;
                Debug.Log("Seize : " + time);
            }

            if (time == 100) {
                OwnerNum = (PlayerController.Player)enemys[0].gameObject.layer;
                GetComponent<Collider2D>().enabled = false;
                enemys.Clear();
                healingTarget.Clear();
                GetComponent<Collider2D>().enabled = true;
                startSeize = false;
                isAlreadyCoroutine = false;
                if (OwnerNum == PlayerController.Player.PLAYER_1)
                    IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.MISSION_EVENT.NODE_CAPTURE_COMPLETE, this, null);
            }
            if (healingTarget.Count > 0) startSeize = false;
            Debug.Log(time);
            circularSlider.ChangeByValue(time);
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator SeizeNeutralBuilding() {
        int targetLayer = 0;
        while (startSeize) {
            foreach (GameObject target in enemys.ToList()) {
                if (target == null) continue;
                if (targetLayer == 0) {
                    targetLayer = target.layer;
                    continue;
                }
                if ((int)OwnerNum != target.layer) {
                    yield return new WaitForSeconds(0.1f);
                    if (targetLayer != target.layer) startSeize = false;
                }
            }
            yield return new WaitForSeconds(0.1f);
            //StartCoroutine(SeizeBuilding());
            startSeize = false;
        }
    }

    bool canSeize() {
        var query =
            from unit in enemys
            where(unit.GetComponent<HeroAI>() != null)
            group unit by unit.GetComponent<UnitAI>().ownerNum;

        int count = 0;
        foreach(var group in query) {
            count++;
        }
        //Debug.Log("그룹 : " + count);
        if(count == 1) {
            //Debug.Log("점령 진행");
            return true;
        }
        else {
            //Debug.Log("적이 감지되어 점령 중지");
            return false;
        }
    }
}

public partial class HealingCenterStation : DefaultStation {

    [SerializeField]
    [ReadOnly] public List<GameObject> enemys;
    [ReadOnly] public List<GameObject> healingTarget;

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == 16) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!enemys.Exists(x => x == collision.gameObject)) enemys.Add(collision.gameObject);
            if(OwnerNum == PlayerController.Player.NEUTRAL) {
                if (seizePlayer != collision.gameObject.layer)
                    startSeize = false;
            }
        }
        if ((collision.gameObject.layer == (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!healingTarget.Exists(x => x == collision.gameObject)) healingTarget.Add(collision.gameObject);
            collision.gameObject.AddComponent<Heal>();
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 16) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            enemys.Remove(collision.gameObject);
        }
        if ((collision.gameObject.layer == (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (healingTarget.Exists(x => x == collision.gameObject)) healingTarget.Remove(collision.gameObject);
            Heal heal = collision.gameObject.GetComponent<Heal>();
            if (heal != null)
                Destroy(heal);
        }
    }
}

