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
    public MapStation currentNode;

    public void SetMove(List<Vector3> MovingPos) {
        if(moving) return;
        this.MovingPos = new List<Vector3>(MovingPos);
        if(this.MovingPos == null) return;
        this.MovingPos.RemoveAt(0);
        if(this.MovingPos.Count == 0) return;
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
        CheckGoal();
    }

    private void CheckGoal() {
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
        moveSpeed = unitAIs[0].moveSpeed * 0.4f;
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
        if(node.gameObject.layer == LayerMask.NameToLayer("Node")) {
            MapStation station = node.gameObject.GetComponent<MapStation>();
            if(station == null) return;
            currentNode = station;
        }
    }

}
