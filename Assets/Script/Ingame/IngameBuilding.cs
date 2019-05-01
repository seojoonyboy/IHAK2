using AI;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class IngameBuilding : SkyNet {
    private float hpOffsetSize = 6;
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
        GiveExp();

        SkeletonAnimation ani = GetComponent<SkeletonAnimation>();
        ani.skeletonDataAsset = IngameHpSystem.Instance.wreckSpine;
        Spine.Animation[] items = ani.skeletonDataAsset.GetSkeletonData(false).Animations.Items;

        ani.Initialize(true);
        ani.AnimationState.SetAnimation(0, items[0], true);

        Debug.Log("HQ 파괴");
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

    protected override void CalculateHealthBar() {
        float percent = HP / MaxHealth;
        if(MaxHealth != 0) {
            healthBar.transform.localScale = new Vector3(hpOffsetSize * percent, 3f, 1f);
        }
    }

    public void SetHp(int amount) {
        MaxHealth = amount;
        HP = amount;
    }

    public override void Init(object data, GameObject gameObject) { }
}
