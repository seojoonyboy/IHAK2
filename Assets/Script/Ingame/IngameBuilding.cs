using AI;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class IngameBuilding : SkyNet {
    public override void Damage(float damage) {
        base.Damage(damage);
    }

    public override void Init(object data) {

    }

    public override void Recover(float amount) {
        base.Recover(amount);
    }

    public override void Die() {
        base.Die();
    }

    public override void ChangeOwner(int newNum) {
        base.ChangeOwner(newNum);
    }

    protected void ObjectActive() {
        Observable.EveryUpdate()
            .Where(_ => HP >= MaxHealth)
            .Subscribe(_ => {
                healthBar.gameObject.SetActive(false);
                healthBar.parent.Find("BackGround").gameObject.SetActive(false);
            }).AddTo(gameObject);
        Observable.EveryUpdate()
            .Where(_ => HP < MaxHealth)
            .Subscribe(_ => {
                healthBar.gameObject.SetActive(true);
                healthBar.parent.Find("BackGround").gameObject.SetActive(true);
            }).AddTo(gameObject);
    }

    public override void Init(object data, GameObject gameObject) { }
}
