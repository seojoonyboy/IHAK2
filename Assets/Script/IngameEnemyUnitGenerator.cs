using DataModules;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IngameEnemyUnitGenerator : MonoBehaviour {
    public List<WaveInfo> waveInfos;
    public int CurrentWave { get; set; }

    [SerializeField] Sprite magma;
    Dictionary<LOCATION, Vector2> locations;
    private Transform parent;

    private bool isTileSet = false;
    private const float TARGET_LOCATION_NUM = 4;

    IEnumerator 
        coroutine,
        innerCoroutine;

    // Use this for initialization
    void Start() {
        CurrentWave = 0;

        coroutine = Spawing();
        StartCoroutine(coroutine);
    }

    void OnDestroy() {
        if(innerCoroutine != null) StopCoroutine(innerCoroutine);
        if(coroutine != null) StopCoroutine(coroutine);
    }

    public void SetLocations(Vector2[] targets, Transform parent) {
        if (targets.Length != TARGET_LOCATION_NUM) return;
        locations = new Dictionary<LOCATION, Vector2>();
        for (int i = 0; i < TARGET_LOCATION_NUM; i++) {
            locations[(LOCATION)i] = targets[i];
        }
        this.parent = parent;
        isTileSet = true;
    }

    IEnumerator Spawing() {
        while(CurrentWave < waveInfos.Count) {
            WaveInfo waveInfo = waveInfos[CurrentWave];
            MS_Time ms = waveInfo.intervals;
            int waitingSecs = ms.min * 60 + ms.sec;
            if (CurrentWave > 0) {
                MS_Time prev_ms = waveInfos[CurrentWave - 1].intervals;
                waitingSecs -= prev_ms.min * 60 + prev_ms.sec;
            }
            innerCoroutine = RemainTime(waitingSecs);
            StartCoroutine(innerCoroutine);

            yield return new WaitForSeconds(waitingSecs);

            foreach (WaveSet set in waveInfo.wave.sets) {
                if(set.type == TYPE.SPELL) {
                    SkillDetail method = set.Prefab.GetComponent<BuildingObject>().data.card.activeSkills[0].method;
                    var ingameCityManager = GetComponent<IngameEnemyGenerator>();
                    ingameCityManager.gameObject.AddComponent<Temple_Damager>().GenerateAttack(method, IngameCityManager.Target.ME);
                    ingameCityManager.gameObject.GetComponent<Temple_Damager>().magma = magma;

                }
                else if(set.type == TYPE.UNIT) {
                    for(int i=0; i<set.num; i++) {
                        SetUnitData(set);
                    }
                }
                yield return new WaitForSeconds(0.3f);
            }
            CurrentWave++;
        }
    }

    private void SetUnitData(WaveSet set) {
        float randomPosX = Random.Range(-10f, 10f);
        float randomPosY = Random.Range(-10f, 10f);

        Card card = AccountManager.Instance.GetCardData(set.id);
        if (card == null) return;

        GameObject unit = Instantiate(set.Prefab, parent);
        UnitAI unitAI = unit.GetComponent<UnitAI>();
        unitAI.SetUnitData(card.unit);
        unit.transform.localPosition = locations[set.genLocation] + new Vector2(randomPosX, randomPosY);
        unit.layer = LayerMask.NameToLayer("EnemyUnit");
        if(card.id != "n_u_0101") {
            GameObject name = unit.transform.Find("Name").gameObject;
            name.SetActive(true);
            name.GetComponent<TextMeshPro>().text = card.unit.name;
        }
    }

    IEnumerator RemainTime(int waitingSecs) {
        float time = waitingSecs;
        Debug.Log(waitingSecs);
        while(time > 0) {
            time -= 1;
            //Debug.Log("적 Wave까지 남은 시간 : " + time);
            yield return new WaitForSeconds(1.0f);
        }
        yield return 0;
    }
}

namespace DataModules {
    [System.Serializable]
    public class WaveInfo {
        public MS_Time intervals;
        public Wave wave;
    }

    [System.Serializable]
    public class Wave {
        public List<WaveSet> sets;
    }

    [System.Serializable]
    public class WaveSet {
        public LOCATION genLocation;
        public TYPE type;
        public GameObject Prefab;
        public int num;
        public string id = "n_u_0101";
    }

    [System.Serializable]
    public class MS_Time {
        public int min;
        public int sec;
    }

    public enum LOCATION {
        LEFT_TOP,
        RIGHT_TOP,
        LEFT_BOTTOM,
        RIGHT_BOTTOM,
        RANDOM
    }
    
    public enum TYPE {
        UNIT,
        SPELL
    }
}