using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameEnemyUnitGenerator : MonoBehaviour {
    public List<WaveInfo> waveInfos;
    public int CurrentWave { get; set; }

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

    IEnumerator Spawing() {
        while(CurrentWave <= waveInfos.Count) {
            WaveInfo waveInfo = waveInfos[CurrentWave];
            MS_Time ms = waveInfo.intervals;
            int waitingSecs = ms.min * 60 + ms.sec;

            innerCoroutine = RemainTime(waitingSecs);
            StartCoroutine(innerCoroutine);

            yield return new WaitForSeconds(waitingSecs);

            foreach (WaveSet set in waveInfo.wave.sets) {
                GameObject unit = Instantiate(set.Prefab);
                yield return new WaitForSeconds(0.3f);
            }

            CurrentWave++;
        }
    }

    IEnumerator RemainTime(int waitingSecs) {
        float time = waitingSecs;
        Debug.Log(waitingSecs);
        while(time > 0) {
            time -= 1;
            Debug.Log("적 Wave까지 남은 시간 : " + time);
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