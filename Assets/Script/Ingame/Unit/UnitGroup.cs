using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;

public class UnitGroup : MonoBehaviour {
    private UnitSpine[] unitAnimations;
    private UnitAI[] unitAIs;
    private List<Vector3> MovingPos;

    private float moveSpeed;
    private bool moving;

    public void SetMove(List<Vector3> MovingPos) {
        this.MovingPos = new List<Vector3>(MovingPos);
        UnitMoveAnimation(true);
        moving = true;
        UnitMoveDirection(MovingPos[0]);
    }

    private void Update() {
        if(!moving) return;
        Moving(Time.deltaTime);
    }

    private void Moving(float time) {
        Vector3 distance3 = transform.position - MovingPos[0];
        transform.Translate(distance3 * time * moveSpeed);
        CheckGoal();
    }

    private void CheckGoal() {
        bool isGoal = Vector3.Distance(transform.position, MovingPos[0]) <= 0.1f;
        if(!isGoal) return;
        MovingPos.RemoveAt(0);
        if(MovingPos.Count == 0) moving = false;
        else UnitMoveDirection(MovingPos[0]);
    }

    private void Start() {
        GetData();
        moving = false;
    }

    private void GetData() {
        unitAnimations = transform.GetComponentsInChildren<UnitSpine>();
        unitAIs = transform.GetComponentsInChildren<UnitAI>();
        //UnitIndividualSet(false);
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
        Vector3 result = transform.position - pos;
        for(int i = 0; i < unitAnimations.Length; i++)
            unitAnimations[i].SetDirection(pos);
    }  

}
