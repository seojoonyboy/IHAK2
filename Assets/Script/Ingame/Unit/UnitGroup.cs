using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using UniRx;
using UniRx.Triggers;

public class UnitGroup : MonoBehaviour {
    private UnitSpine[] unitAnimations;
    private UnitAI[] unitAIs;
    private List<Vector3> MovingPos;

    private float moveSpeed;
    private bool moving = false;
    private bool attacking = false;
    public MapStation currentStation;
    public MapNode currentNode;
    private List<GameObject> enemyGroup;
    private GameObject enemyBuilding;
    private Collider2D clickCol;
    private bool directionOpen = false;

    private int maxMinionNum;
    private int currentMinionNum {get {return transform.childCount -2;}}
    private string minionType;

    private void Start() {
        clickCol = GetComponent<CircleCollider2D>();
        var clickGroup = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));
        clickGroup.RepeatUntilDestroy(gameObject).Where(_ => !moving && ClickGroup()).Subscribe(_ => checkWay());
        GetData();
        SetMinionData();
        UnitMoveAnimation(false);
        if (!moving) return;
        MoveStart();
    }

    public void SetMove(List<Vector3> pos) {
        if(moving) return;
        MovingPos = new List<Vector3>(pos);
        if(MovingPos == null || MovingPos.Count == 0) return;
        MovingPos.RemoveAt(0);
        if(MovingPos.Count == 0) return;
        MoveStart();
    }

    private void MoveStart() {
        moving = true;
        if(unitAIs == null) return;
        UnitMoveAnimation(true);
        UnitMoveDirection(MovingPos[0]);
    }

    private void Update() {
        MoveUpdate(Time.deltaTime);
        //AttackUpdate(Time.deltaTime);
    }

    private void MoveUpdate(float time) {
        if(!moving) return;
        Moving(time);
    }
    private void AttackUpdate(float time) {
        if(!attacking) return;
        transform.position = transform.GetChild(1).position;
    }

    private void Moving(float time) {
        Vector3 distance3 = MovingPos[0] - transform.position;
        transform.Translate(distance3.normalized * time * moveSpeed);
        CheckStationReached();
    }

    private void CheckStationReached() {
        bool isGoal = Vector3.Distance(transform.position, MovingPos[0]) <= 0.5f;
        if(!isGoal) return;
        MovingPos.RemoveAt(0);
        if(MovingPos.Count == 0) MoveEnd();
        else UnitMoveDirection(MovingPos[0]);
    }

    private void MoveEnd() {
        moving = false;
        UnitMoveAnimation(false);
        MovingPos = null;
        AmIinHQ();
    }

    private void AmIinHQ() {
        UnitAI unit = transform.GetComponentInChildren<UnitAI>();
        if(unit == null) return;
        int unitLayer = unit.gameObject.layer;
        bool isPlayer1 = unitLayer == LayerMask.NameToLayer("PlayerUnit");
        bool youAreinHQ = currentStation.mapPostion == (isPlayer1 ? EnumMapPosition.S12 : EnumMapPosition.S10);
        if(!youAreinHQ) return;
        AttackHQ(isPlayer1 ? "EnemyCity" : "PlayerCity");
    }

    private void AttackHQ(string name) {
        GameObject group = GameObject.Find(name);
        Transform hq = group.transform.GetChild(0).Find("Tile[2,2]");
        enemyBuilding = hq.gameObject;
        enemyGroup = new List<GameObject>();
        this.enabled = false;
        attacking = true;
        UnitIndividualSet(true);
    }

    

    private void GetData() {
        unitAnimations = transform.GetComponentsInChildren<UnitSpine>();
        unitAIs = transform.GetComponentsInChildren<UnitAI>();
        UnitIndividualSet(false);
        moveSpeed = unitAIs[0].moveSpeed;
    }

    public void ResetData() {
        unitAnimations = transform.GetComponentsInChildren<UnitSpine>();
        unitAIs = transform.GetComponentsInChildren<UnitAI>();
    }

    private void SetMinionData() {
        HeroAI hero = unitAIs[0].GetComponent<HeroAI>();
        Minion minionData = hero.unitCard.baseSpec.unit.minion;
        maxMinionNum = minionData.count;
        minionType = minionData.type;
        Debug.Log(IsMinionMax());
        Debug.Log(MinionType());
    }

    public bool IsMinionMax() {
        if(IsHeroDead()) return true;
        return maxMinionNum == currentMinionNum;
    }

    private bool IsHeroDead() {
        HeroAI hero = transform.GetChild(1).GetComponent<HeroAI>();
        if(hero == null) return true;
        return false;
    }

    public string MinionType() {
        return minionType;
    }

    private void UnitIndividualSet(bool attack) {
        for(int i = 0; i < unitAIs.Length; i++) {
            if(unitAIs[i] == null) continue;
            unitAIs[i].enabled = attack;
        }
    }

    private void UnitMoveAnimation(bool move) {
        for(int i = 0; i < unitAnimations.Length; i++) {
            if(unitAIs[i] == null) continue;
            if(move) unitAnimations[i].Move();
            else     unitAnimations[i].Idle();
        }
    }

    private void UnitMoveDirection(Vector3 pos) {
        Vector3 result = pos - transform.position;
        for(int i = 0; i < unitAnimations.Length; i++) {
            if(unitAIs[i] == null) continue;
            unitAnimations[i].SetDirection(result);
        }
    }

    private void OnTriggerEnter2D(Collider2D node) {
        if(SetNodePosition(node)) return;
    }

    private bool SetNodePosition(Collider2D node) {
        if(node.gameObject.layer != LayerMask.NameToLayer("Node")) return false;
        currentNode = node.gameObject.GetComponent<MapNode>();
        MapStation station = node.gameObject.GetComponent<MapStation>();
        if(station == null) return true;
        if (currentStation != null) {
            foreach (MapStation.NodeDirection nextNode in currentStation.adjNodes.Keys) {
                transform.GetChild(0).GetChild((int)nextNode).gameObject.SetActive(false);
            }
        }
        currentStation = station;
        foreach (MapStation.NodeDirection nextNode in currentStation.adjNodes.Keys) {
            transform.GetChild(0).GetChild((int)nextNode).gameObject.SetActive(true);
        }
        return true;
    }

    public void UnitHittedOrFound(Transform enemy) {
        if(attacking) return;
        PrepareBattle(enemy);
    }

    private void PrepareBattle(Transform enemy) {
        this.enabled = false;
        attacking = true;
        MonsterCheck(enemy);
        UnitCheck(enemy);
        BuildingCheck(enemy);

        UnitIndividualSet(true);
    }

    private void MonsterCheck(Transform enemy) {
        MonsterAI monster = enemy.GetComponent<MonsterAI>();
        if(monster == null) {
            return;
        }
        if(monster.tower.GetType() == typeof(CreepStation)) {
            enemyGroup = ((CreepStation)monster.tower).monsters;
        }

        if(monster.tower.GetType() == typeof(BaseCampStation)) {
            enemyGroup = ((BaseCampStation)monster.tower).monsters;
            enemyBuilding = ((BaseCampStation)monster.tower).towerComponent.gameObject;
        }
        return;
    }

    private void UnitCheck(Transform enemy) {
        UnitAI unit = enemy.GetComponent<UnitAI>();
        if(unit == null) return;
        //TODO : 추후에 유닛끼리 대결할 때 유닛들의 리스트를 가져오기
        return;
    }

    private void BuildingCheck(Transform enemy) {
        IngameBuilding detector = enemy.GetComponent<IngameBuilding>();
        if(detector == null) return;
        BaseCampStation baseCamp = detector.GetComponentInParent<BaseCampStation>();
        if(baseCamp != null) {
            enemyGroup = baseCamp.monsters;
            enemyBuilding = (baseCamp.towerComponent.gameObject);
            return;
        }
        TowerStation tower = detector.GetComponentInParent<TowerStation>();
        if(tower != null) {
            if(enemyGroup == null) enemyGroup = new List<GameObject>();
            enemyBuilding = tower.towerComponent.gameObject;
            return;
        }
        Debug.LogWarning("어떤 건물에 있는 놈인지 이종욱에게 알려주세요");
        return;
    }

    private bool CheckEnemyLeft() {
        if(enemyGroup == null) return false;
        if(enemyGroup.Count != 0 || enemyBuilding != null) return true;
        enemyGroup = null;
        enemyBuilding = null;
        FinishBattle();
        return false;
    }

    public Transform GiveMeEnemy(Transform myTransform) {
        if(!CheckEnemyLeft()) return null;
        Transform target = null;
        CheckEnemyGroup(myTransform, ref target);
        if(target != null) return target;
        CheckEnemyBuilding(ref target);
        return target;
    }

    private void CheckEnemyGroup(Transform myTransform, ref Transform target) {
        if(enemyGroup == null) return;
        float shortLength = float.MaxValue;
        for(int i = 0; i < enemyGroup.Count; i++) {
            if(enemyGroup[i] == null) {
                enemyGroup.RemoveAt(i--);
                continue;
            }
            Transform next = enemyGroup[i].transform;
            float length = Vector3.Distance(myTransform.position, next.position);
            CheckDistance(ref target, ref shortLength, next, length);
        }
    }

    private void CheckEnemyBuilding(ref Transform target) {
        if(enemyBuilding == null) return;
        if(enemyBuilding.GetComponent<TileObject>()) {
            target = enemyBuilding.transform;
            return;
        }
        if(enemyBuilding.GetComponent<IngameBuilding>().HP <= 0) {
            enemyBuilding = null;
            return;
        }
        target = enemyBuilding.transform;
    }

    private void CheckDistance(ref Transform target, ref float shortLength, Transform next, float length) {
        if(target == null) {
            target = next;
            shortLength = length;
        }
        else if(shortLength > length) {
            target = next;
            shortLength = length;
        }
    }

    private void FinishBattle() {
        enemyGroup = null;
        this.enabled = true;
        attacking = false;
        UnitIndividualSet(false);
        if(IsHeroDead()) Destroy(gameObject, 5f);
        if(!moving) return;
        UnitMoveAnimation(true);
        UnitMoveDirection(MovingPos[0]);
    }

    public void UnitDead() {
        //TODO : 테스트 필요 게임오브젝트 파괴 명령 해도 바로 안사라지는 문제가 있어서 -1로 해야하는데 일단 0으로 세팅
        if(currentMinionNum == 0) Destroy(gameObject);
    }

    private bool ClickGroup() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);
            LayerMask mask = LayerMask.NameToLayer("Direction");
            RaycastHit2D hits = Physics2D.Raycast(mousePos, transform.forward, mask);
            Debug.Log(hits.collider);
            //foreach (RaycastHit2D target in hits) {
            //    if (target.collider == clickCol)
            //        return true;
            //    if (target.collider.gameObject.layer == 31) {
            //        int index = target.collider.transform.GetSiblingIndex();
            //        List<Vector3> path = new List<Vector3>();
            //        path.Add(currentStation.transform.position);
            //        path.Add(currentStation.adjNodes[(MapStation.NodeDirection)index].transform.position);
            //        SetMove(path);
            //        return true;
            //    }
            //}
            if (!hits) {
                if (directionOpen) checkWay();
                return false;
            }
            if (hits.collider.attachedRigidbody == transform.GetChild(1).GetComponent<Rigidbody2D>())
                return true;
            if (hits.collider == clickCol)
                return true;
            if (hits.transform.parent.GetComponent<CircleCollider2D>() == clickCol)
                return true;
            //if (hits.collider == clickCol)

            else if (hits.collider.gameObject.layer == 31) {
                int index = hits.collider.transform.GetSiblingIndex();
                List<Vector3> path = new List<Vector3>();
                path.Add(currentStation.transform.position);
                path.Add(currentStation.adjNodes[(MapStation.NodeDirection)index].transform.position);
                SetMove(path);
                return true;
            }
            else return false;
        }
        return false;
    }

    public void checkWay() {
        directionOpen = !directionOpen;
        foreach (MapStation.NodeDirection node in currentStation.adjNodes.Keys) {
            gameObject.GetComponent<CircleCollider2D>().enabled = !directionOpen;
            transform.GetChild(0).gameObject.SetActive(directionOpen);
        }
    }
}