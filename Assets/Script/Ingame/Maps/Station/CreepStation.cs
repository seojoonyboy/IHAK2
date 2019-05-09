using ingameUIModules;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;


public partial class CreepStation : DefaultStation {

    [SerializeField] [ReadOnly] protected bool startSeize = false;
    [SerializeField] [ReadOnly] public GameObject stationObject;

    // Use this for initialization
    void Start () {
        OwnerNum = PlayerController.Player.NEUTRAL;
        StationIdentity = StationBasic.StationState.Creep;
        
        pivotTime = 60;
        intervalTime = new ReactiveProperty<int>(pivotTime);        
        targets = new List<GameObject>();
        SetMonsters();
        SetCreepPoint();
        MonstersReset(false);
        PostRespawnTimer();        
    }

    private void SetCreepPoint() {
        stationObject = ConstructManager.Instance.gameObject.GetComponent<BuildingImages>().pointObjects[0];
        stationObject = Instantiate(stationObject, transform);
        stationObject.transform.SetAsLastSibling();
    }

    private void LateUpdate() {
        if (!startSeize && monsters.Count == 0 && targets.Count > 0 ) {
            startSeize = true;
            StartCoroutine(FindOwner());
        }
    }

    public override void DestroyEnteredTarget(GameObject unitObj) {
        if (targets.Contains(unitObj)) targets.Remove(unitObj);
    }

    
    IEnumerator FindOwner() {
        int targetLayer = 0;
        while (startSeize) {
            foreach (GameObject target in targets.ToList()) {
                if(target == null) continue;
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
            OwnerNum = (PlayerController.Player)targetLayer;
            GetComponent<Collider2D>().enabled = false;
            targets.Clear();
            GetComponent<Collider2D>().enabled = true;
            startSeize = false;
            checkFlag = true;
            intervalTime.Value = pivotTime;
            SpawnPlayerMinion();
            IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.MISSION_EVENT.NODE_CAPTURE_COMPLETE, this, null);
        }
    }
    
    public void ChangeOwner() { }
}

public partial class CreepStation {
    [SerializeField] [ReadOnly] public List<GameObject> targets;
    public Transform monsterParent;
    public List<GameObject> monsters;
    public Pool[] pools = new Pool[1];
    [SerializeField] [ReadOnly] int poolLv = 0;
    public CircularSlider occupySlider;
    [SerializeField] [ReadOnly] int maxMonsterNum;
    private float occupyPercentageVal;
    public void SetMonsters() {
        monsters = new List<GameObject>();
        monsterParent = transform.parent.parent.Find("Monsters");
        Instantiate(Resources.Load("Prefabs/Monsters/MonsterPos") as GameObject, transform);

        List<Transform> wayPoints = new List<Transform>();
        foreach (Transform wayPoint in transform.GetChild(0)) {
            wayPoints.Add(wayPoint);
        }

        GameObject goblin = Resources.Load("Prefabs/Monsters/Goblin") as GameObject;
        goblin.GetComponent<MonsterAI>().Init(AccountManager.Instance.neutralMonsterDatas.Find(x => x.id == "npc_monster_01001"));
        goblin.GetComponent<MonsterAI>().expPoint = 20;

        var creeps = AccountManager.Instance.mission.creeps;
        foreach (DataModules.MonsterData monsterData in creeps) {
            if (monsterData.creep.id == "npc_monster_01001") {
                int monsterCount = monsterData.count;
                for (int i = 0; i < monsterCount; i++) {
                    GameObject generateMonster = Instantiate(goblin, monsterParent);
                    generateMonster.transform.position = transform.GetChild(0).GetChild(i).position;

                    generateMonster.GetComponent<StateController>().SetupAI(true, wayPoints);
                    generateMonster.GetComponent<MonsterAI>().tower = this;
                    monsters.Add(generateMonster);
                }
                occupyPercentageVal = 100 / monsterCount;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == 16) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            if (!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 16) return;
        if ((collision.gameObject.layer != (int)OwnerNum) && collision.GetComponent<UnitAI>() != null) {
            foreach(GameObject monster in monsters) {
                if(monster.GetComponent<StateController>().chaseTarget == collision.transform) {
                    monster.GetComponent<StateController>().chaseTarget = null;
                }
            }
            targets.Remove(collision.gameObject);
        }
    }

    void Update() {
        foreach (GameObject monster in monsters.ToList()) {
            if (monster == null) {
                monsters.Remove(monster);
            }
        }
        if (targets != null && targets.Count == 0) {
            foreach (GameObject monster in monsters) {
                monster.GetComponent<StateController>().chaseTarget = null;
            }
        }
    }

    public void TargetDie() { }

    public void MonsterDie(GameObject monster) {
        monsters.Remove(monster);
        try {
            occupySlider.IncreaseByPercentage(occupyPercentageVal);
        }
        catch (NullReferenceException ne) {
            Debug.Log("크립지점");
        }
    }

    public void MonstersReset(bool isTierUp) {
        //if (isTierUp) poolLv++;

        //Pool selPool = pools[poolLv];

        //List<Transform> wayPoints = new List<Transform>();
        //foreach (Transform wayPoint in transform.GetChild(0)) {
        //    wayPoints.Add(wayPoint);
        //}

        //foreach (Set set in selPool.sets) {
        //    for (int i = 0; i < set.num; i++) {
        //        GameObject instantiatedMonster = Instantiate(set.monster, monsterParent);
        //        instantiatedMonster.transform.position = transform
        //            .GetChild(0)
        //            .GetChild(i)
        //            .transform
        //            .position;
                
        //        instantiatedMonster.GetComponent<StateController>().SetupAI(true, wayPoints);
        //        instantiatedMonster.GetComponent<MonsterAI>().tower = this;
        //        instantiatedMonster.GetComponent<MonsterAI>().ownerNum = OwnerNum;
        //        monsters.Add(instantiatedMonster);
        //    }
        //}
    }

    private void SetOccupySlider() {
        occupySlider = (Instantiate(Resources.Load("Prefabs/OccupySlider"), transform) as GameObject).GetComponent<CircularSlider>();
        occupySlider.Reset();

    }

    private void Occupied() {

    }

    [System.Serializable]
    public class Pool {
        public List<Set> sets;
    }

    [System.Serializable]
    public class Set {
        public int num;
        public GameObject monster;
    }
}

public partial class CreepStation {

    public void PostRespawnTimer() {
        var oneSecond = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1)).Publish().RefCount();
        oneSecond.Where(_ => (monsters.Count <= 0) && OwnerNum != PlayerController.Player.NEUTRAL).Subscribe(_ => { intervalTime.Value--; }).AddTo(this);
        oneSecond.Where(_ => (intervalTime.Value <= 0) && OwnerNum != PlayerController.Player.NEUTRAL).Subscribe(_ => { RespawnMonster(); intervalTime.Value = pivotTime; }).AddTo(this);
    }

    public void SpawnPlayerMinion() {
    }


    public void RespawnMonster() {
        List<Transform> wayPoints = new List<Transform>();
        foreach (Transform wayPoint in transform.GetChild(0)) {
            wayPoints.Add(wayPoint);
        }

        GameObject goblin = Resources.Load("Prefabs/Monsters/Goblin") as GameObject;
        goblin.GetComponent<MonsterAI>().Init(AccountManager.Instance.neutralMonsterDatas.Find(x => x.id == "npc_monster_01001"));
        goblin.GetComponent<MonsterAI>().expPoint = 20;

        var creeps = AccountManager.Instance.mission.creeps;
        foreach (DataModules.MonsterData monsterData in creeps) {
            if (monsterData.creep.id == "npc_monster_01001") {
                int monsterCount = monsterData.count;
                for (int i = 0; i < monsterCount; i++) {
                    GameObject generateMonster = Instantiate(goblin, monsterParent);
                    generateMonster.transform.position = transform.GetChild(0).GetChild(i).position;

                    generateMonster.GetComponent<StateController>().SetupAI(true, wayPoints);
                    generateMonster.GetComponent<MonsterAI>().tower = this;
                    monsters.Add(generateMonster);
                }
                occupyPercentageVal = 100 / monsterCount;
            }
        }
        checkFlag = false;
        OwnerNum = PlayerController.Player.NEUTRAL;
        occupySlider.Reset();
        intervalTime.Value = pivotTime;
    }
    


}