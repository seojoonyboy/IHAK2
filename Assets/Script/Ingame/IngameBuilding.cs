using AI;
using System.Collections;
using System.Collections.Generic;
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
}
