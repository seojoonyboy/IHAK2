using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataModules;
using UniRx;
using UniRx.Triggers;

public class UnitGroup : MonoBehaviour {
    private UnitSpine[] unitAnimations;
    private UnitAI[] unitAIs;
    private List<Vector3> MovingPos;
    public List<GameObject> myGroupList;

    private float moveSpeed;
    private bool moving = false;
    private bool attacking = false;
    private bool escaping = false;
    public MapStation currentStation;
    public MapNode currentNode;
    private List<GameObject> enemyGroup;
    private GameObject enemyBuilding;
    private Collider2D clickCol;
    private bool directionOpen = false;

    private int maxMinionNum;
    public int currentMinionNum {get {return transform.childCount -3;}}
    private string minionType;

    public bool IsMoving { get { return moving; } }
    public bool Attacking { get { return attacking; } }
    public bool IsClickable { 
        get {
            if(!moving) return true;
            bool fightingInStation = currentNode.GetComponent<MapStation>() != null && attacking;
            return fightingInStation;
        } 
    }

    private void Start() {
        clickCol = GetComponent<CircleCollider2D>();
        var clickGroup = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));
        clickGroup.RepeatUntilDestroy(gameObject).Where(_ => IsClickable && ClickGroup() && !Canvas.FindObjectOfType<ToggleGroup>().AnyTogglesOn()).Subscribe(_ => checkWay());
        GetData();
        SetMinionData();
        UnitMoveAnimation(false);
        StartCoroutine(MinimapSpawnAlert());
        if (!moving) return;
        MoveStart();
    }

    public void SetMove(List<Vector3> pos) {
        if(!IsClickable) return;
        IsNeedtoEscapse();
        MovingPos = new List<Vector3>(pos);
        if(MovingPos == null || MovingPos.Count == 0) return;
        MovingPos.RemoveAt(0);
        if(MovingPos.Count == 0) return;
        MoveStart();
    }

    private void IsNeedtoEscapse() {
        if(!attacking) return;
        escaping = true;
        moving = false;
        FinishBattle();
    }

    private void MoveStart() {
        moving = true;
        if(unitAIs == null) return;
        UnitMoveAnimation(true);
        UnitMoveDirection(MovingPos[0]);

        if(myGroupList[0].gameObject.layer == 10) {
            IngameSceneEventHandler.Instance.PostNotification(IngameSceneEventHandler.MISSION_EVENT.MOVE_COMPLETE, this, null);
        }
    }

    private void Update() {
        MoveUpdate(Time.deltaTime);
    }

    private void MoveUpdate(float time) {
        if(!moving) return;
        if(attacking) return;
        Moving(time);
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
        escaping = false;
        UnitMoveAnimation(false);
        MovingPos = null;
        AmIinHQ();
    }

    private void AmIinHQ() {
        //TODO : 미션으로 인해 맵이 바뀌는데 적의 위치가 무조건 S12라고는 안할듯
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
        Transform hq = group.transform.Find("HQ(Clone)");
        enemyBuilding = hq.gameObject;
        enemyGroup = new List<GameObject>();
        attacking = true;
        UnitIndividualSet(true);
    }

    

    private void GetData() {
        DataSetting();
        UnitIndividualSet(false);
        moveSpeed = unitAIs[0].moveSpeed;
    }

    public void ResetData() {
        DataSetting();
    }

    private void DataSetting() {
        unitAnimations = transform.GetComponentsInChildren<UnitSpine>();
        unitAIs = transform.GetComponentsInChildren<UnitAI>();
        SetGroupList();
    }

    private void SetGroupList() {
        myGroupList = new List<GameObject>();
        for(int i = 0; i < unitAIs.Length; i++) {
            if(unitAIs[i] == null) continue;
            myGroupList.Add(unitAIs[i].gameObject);
        }
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
        HeroAI hero = transform.GetChild(2).GetComponent<HeroAI>();
        if(hero == null) return true;
        return false;
    }

    public string MinionType() {
        return minionType;
    }

    private void UnitIndividualSet(bool attack) {
        for(int i = 0; i < unitAIs.Length; i++) {
            if(unitAIs[i] == null) continue;
            unitAIs[i].Battle(attack);
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
        if(escaping) return;
        PrepareBattle(enemy);
    }

    private void PrepareBattle(Transform enemy) {
        attacking = true;
        MonsterCheck(enemy);
        UnitCheck(enemy);
        BuildingCheck(enemy);
        StartCoroutine(MinimapBattleAlart());
        UnitIndividualSet(true);
    }

    IEnumerator MinimapSpawnAlert() {
        if(unitAIs[0].gameObject.layer != 10) {
            transform.GetChild(1).gameObject.SetActive(false);
            yield break;
        }
        int count = 0;
        while (true) {
            transform.GetChild(1).gameObject.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            transform.GetChild(1).gameObject.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            count++;
            if (count == 3)
                break;
        }
    }

    IEnumerator MinimapBattleAlart() {
        if(unitAIs[0].gameObject.layer != 10) yield break;
        while (attacking) {
            transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
            if (!attacking) {
                transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                break;
            }
            yield return new WaitForSeconds(0.5f);
            transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
            yield return new WaitForSeconds(0.2f);
        }
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
        enemyGroup = unit.myGroup.myGroupList;
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
        if(escaping) return null;
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
        attacking = false;
        UnitIndividualSet(false);
        PositionReset();
        if(IsHeroDead()) Destroy(gameObject, 5f);
        if(!moving) return;
        UnitMoveAnimation(true);
        UnitMoveDirection(MovingPos[0]);
    }

    private void PositionReset() {
        if(IsHeroDead()) return;
        int count = transform.childCount;
        Transform[] childrens = new Transform[count];
        for(int i = 0; i < count; i++) childrens[i] = transform.GetChild(i);
        HeroAI hero = transform.GetComponentInChildren<HeroAI>();
        transform.DetachChildren();
        transform.position = hero.transform.position;
        foreach (var item in childrens) {
            item.SetParent(transform, true);
            if(item.GetComponent<UnitAI>() != null) continue;
            item.localPosition = Vector3.zero;
        }
    }

    public void UnitDead(GameObject unit) {
        myGroupList.Remove(unit);
        if(myGroupList.Count == 0) Destroy(gameObject);
    }

    private bool ClickGroup() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            LayerMask mask = (1 << LayerMask.NameToLayer("Direction")) | (1 << LayerMask.NameToLayer("UnitGroup"));
            RaycastHit2D hits = Physics2D.Raycast(new Vector2(mousePos.x, mousePos.y), Vector2.zero, Mathf.Infinity, mask);
            if (!hits) {
                if (directionOpen) checkWay();
                return false;
            }
            if (hits.collider.transform != transform && hits.collider.transform.parent != transform && hits.collider.transform.parent.parent != transform) return false;
            if (hits.collider.transform.childCount > 0 && hits.collider.transform.GetChild(2).gameObject.layer != 10) return false;
            if (hits.collider.attachedRigidbody == transform.GetChild(2).GetComponent<Rigidbody2D>())
                return true;
            if (hits.collider == clickCol)
                return true;

            if (hits.transform.parent.GetComponent<CircleCollider2D>() == clickCol)
                return true;

            else if (hits.collider.gameObject.layer == 31) {
                if (!directionOpen) return false;
                int index = hits.collider.transform.GetSiblingIndex();
                List<Vector3> path = new List<Vector3>();
                path.Add(currentStation.transform.position);
                path.Add(currentStation.adjNodes[(MapStation.NodeDirection)index].transform.position);
                SetMove(path);
                if (!PlayerController.Instance.FirstMove)
                    PlayerController.Instance.FirstMove = true;
                return true;
            }
            else return false;
        }
        return false;
    }

    public void checkWay() {
        if (PlayerController.Instance.ClickedUnitgroup == null)
            PlayerController.Instance.ClickedUnitgroup = gameObject;
        else if (PlayerController.Instance.ClickedUnitgroup == gameObject)
            PlayerController.Instance.ClickedUnitgroup = null;
        else
            return;
        directionOpen = !directionOpen;
        foreach (MapStation.NodeDirection node in currentStation.adjNodes.Keys) {
            gameObject.GetComponent<CircleCollider2D>().enabled = !directionOpen;
            transform.GetChild(0).gameObject.SetActive(directionOpen);
        }
    }
}