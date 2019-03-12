using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temple_Damager : MonoBehaviour {
    IngameSceneEventHandler ingameSceneEventHandler;
    IEnumerator coroutine;
    public Sprite magma;

    private int damageAmount;
    private int interval;
    private int maintainTime;
    private int targetNum;

    private int[] rndTargets;
    private int[] demoTileIndex = { 6, 7, 8, 11, 12, 13, 16, 17, 18 };
    private int[] myTileIndex = { 6, 7, 8, 11, 12, 13, 16, 17, 18 };
    private IngameCityManager cityManager;

    void Awake() {
        ingameSceneEventHandler = IngameSceneEventHandler.Instance;
        cityManager = GetComponent<IngameCityManager>();
    }

    public void GenerateAttack(DataModules.SkillDetail detail, IngameCityManager.Target target) {
        string args = detail.args;
        string[] arr = args.Split(',');
        int.TryParse(arr[0], out targetNum);
        int.TryParse(arr[1], out maintainTime);
        int.TryParse(arr[2], out interval);
        int.TryParse(arr[3], out damageAmount);

        switch (target) {
            case IngameCityManager.Target.ENEMY_1:
                rndTargets = RndNumGenerator.getRandomInt(targetNum, demoTileIndex);
                break;
            case IngameCityManager.Target.ME:
                rndTargets = RndNumGenerator.getRandomInt(targetNum, myTileIndex);
                break;
        }
        MakeDisaster(target);
    }

    public void MakeDisaster(IngameCityManager.Target target) {
        coroutine = Damage(interval, maintainTime, target);
        StartCoroutine(coroutine);
    }

    IEnumerator Damage(float interval = 1.0f, int loopCount = 0, IngameCityManager.Target target = IngameCityManager.Target.ENEMY_1) {
        GameObject usingSkill = new GameObject();
        usingSkill.name = "UsingSkill";
        Transform parent;
        if(target == IngameCityManager.Target.ENEMY_1)
            parent = cityManager.enemyBuildingsInfo[0].gameObject.transform.parent.parent;
        else
            parent = cityManager.myBuildingsInfo[0].gameObject.transform.parent.parent;
        usingSkill.transform.SetParent(parent);
        usingSkill.transform.SetAsLastSibling();
        int count = loopCount;
        while (count > 0) {
            yield return new WaitForSeconds(interval);
            object[] parms = new object[3];
            parms[0] = target;
            parms[1] = rndTargets;
            parms[2] = (int)Mathf.Round(damageAmount * cityManager.myBuildings_mags[3].magnfication);
            GenerateSprite(rndTargets, target);
            ingameSceneEventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.TAKE_DAMAGE, null, parms);
            count--;
        }
        Destroy(GetComponent<Temple_Damager>());
        Destroy(usingSkill);
    }

    private void GenerateSprite(int[] parm, IngameCityManager.Target target) {
        cityManager = GetComponent<IngameCityManager>();
        for(int i = 0; i < parm.Length; i++) {
            var result = cityManager.myBuildingsInfo.Find(x => x.tileNum == parm[i]);
            if (result == null) return;

            Transform pos = result.gameObject.transform;
            switch (target) {
                case IngameCityManager.Target.ENEMY_1:
                    pos = cityManager.enemyBuildingsInfo.Find(x => x.tileNum == parm[i]).gameObject.transform;
                    break;
                case IngameCityManager.Target.ME:
                    pos = cityManager.myBuildingsInfo.Find(x => x.tileNum == parm[i]).gameObject.transform;
                    break;
            }
            
            GameObject magmaObject = new GameObject("magma");
            magmaObject.transform.SetParent(pos, false);
            SpriteRenderer magmaRender = magmaObject.AddComponent<SpriteRenderer>();
            magmaRender.sprite = magma;
            if(pos.GetComponent<SpriteRenderer>() != null)
                magmaRender.sortingOrder = pos.GetComponent<SpriteRenderer>().sortingOrder + 1;
            else
                magmaRender.sortingOrder = pos.GetComponent<MeshRenderer>().sortingOrder + 1;
            Destroy(magmaObject, 1f);
        }
        
    }

    void OnDestroy() {
        StopCoroutine(coroutine);
    }
}
