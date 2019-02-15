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

    void Awake() {
        ingameSceneEventHandler = IngameSceneEventHandler.Instance;
    }

    public void GenerateAttack(DataModules.SkillDetail detail) {
        string args = detail.args;
        string[] arr = args.Split(',');
        int.TryParse(arr[0], out targetNum);
        int.TryParse(arr[1], out maintainTime);
        int.TryParse(arr[2], out interval);
        int.TryParse(arr[3], out damageAmount);

        rndTargets = RndNumGenerator.getRandomInt(targetNum, demoTileIndex);

        MakeDisaster();
    }

    public void MakeDisaster() {
        coroutine = Damage(interval, maintainTime);
        StartCoroutine(coroutine);
    }

    IEnumerator Damage(float interval = 1.0f, int loopCount = 0) {
        int count = loopCount;
        while (count > 0) {
            yield return new WaitForSeconds(interval);
            object[] parms = new object[3];
            parms[0] = IngameCityManager.Target.ENEMY_1;
            parms[1] = rndTargets;
            parms[2] = damageAmount;
            GenerateSprite(rndTargets);
            ingameSceneEventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.TAKE_DAMAGE, null, parms);
            count--;
        }
        Destroy(GetComponent<Temple_Damager>());
    }

    private void GenerateSprite(int[] parm) {
        var cityManager = FindObjectOfType<IngameCityManager>();
        for(int i = 0; i < parm.Length; i++) {
            Transform pos = cityManager.enemyBuildingsInfo[parm[i]].gameObject.transform;
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
