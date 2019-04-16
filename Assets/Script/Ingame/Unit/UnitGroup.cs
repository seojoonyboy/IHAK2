using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;

public class UnitGroup : MonoBehaviour {
    private UnitSpine[] unitAnimations;
    private UnitAI[] unitAIs;
    private List<Vector3> MovingPos;

    private float moveSpeed;
    private bool moving = false;
    private bool attacking = false;
    public MapStation currentStation;
    public MapNode currentNode;
    public Transform enemyGroup;

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
        if(!moving) return;
        Moving(Time.deltaTime);
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
    }

    private void Start() {
        GetData();
        UnitMoveAnimation(false);
        if(!moving) return;
        MoveStart();
    }

    private void GetData() {
        unitAnimations = transform.GetComponentsInChildren<UnitSpine>();
        unitAIs = transform.GetComponentsInChildren<UnitAI>();
        UnitIndividualSet(false);
        moveSpeed = unitAIs[0].moveSpeed;
    }

    private void UnitIndividualSet(bool attack) {
        for(int i = 0; i < unitAIs.Length; i++)
            unitAIs[i].enabled = attack;
    }

    private void UnitMoveAnimation(bool move) {
        for(int i = 0; i < unitAnimations.Length; i++) 
            if(move) unitAnimations[i].Move();
            else     unitAnimations[i].Idle();
    }

    private void UnitMoveDirection(Vector3 pos) {
        Vector3 result = pos - transform.position;
        for(int i = 0; i < unitAnimations.Length; i++)
            unitAnimations[i].SetDirection(result);
    }

    private void OnTriggerEnter2D(Collider2D node) {
        if(SetNodePosition(node)) return; 
        //TODO : 적대 그룹을 만날 경우 - if 내용 변경 필요
        if(attacking) {
            PrepareBattle(node.transform);
        }
    }

    private bool SetNodePosition(Collider2D node) {
        if(node.gameObject.layer != LayerMask.NameToLayer("Node")) return false;
        currentNode = node.gameObject.GetComponent<MapNode>();
        MapStation station = node.gameObject.GetComponent<MapStation>();
        if(station == null) return true;
        currentStation = station;
        return true;
    }

    private void PrepareBattle(Transform group) {
        this.enabled = false;
        attacking = true;
        enemyGroup = group;
        UnitIndividualSet(true);
    }

    private bool CheckEnemyLeft() {
        if(enemyGroup == null) return false;
        if(enemyGroup.childCount != 0) return true;
        FinishBattle();
        return false;
    }

    public Transform GiveMeEnemy(Transform myTransform) {
        //TODO : 자기 유닛위치에 가까운 유닛으로 변경 필요
        if(CheckEnemyLeft()) return enemyGroup.GetChild(0);
        return null;
    }

    private void FinishBattle() {
        enemyGroup = null;
        this.enabled = true;
        attacking = false;
        UnitIndividualSet(false);
    }
}