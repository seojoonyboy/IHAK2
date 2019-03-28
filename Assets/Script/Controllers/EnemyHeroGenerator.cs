using DataModules;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyHeroGenerator : MonoBehaviour {
    public List<Wave> waves;
    private int currentWave;

    IEnumerator
        coroutine,
        innerCoroutine;

    public List<GameObject> generatedHeroes = new List<GameObject>();

    public int CurrentWave {
        get {
            return currentWave;
        }
        set {
            currentWave = value;
            IngameAlarm.instance.SetAlarm("적 영웅이 소환되었습니다!");
        }
    }

    // Use this for initialization
    void Start() {
        coroutine = Spawning();
        StartCoroutine(coroutine);
    }

    // Update is called once per frame
    void Update() {

    }

    void OnDestroy() {
        if (innerCoroutine != null) StopCoroutine(innerCoroutine);
        if (coroutine != null) StopCoroutine(coroutine);
    }

    IEnumerator Spawning() {
        while(CurrentWave < waves.Count) {
            Wave wave = waves[CurrentWave];
            Time ms = wave.invokeTime;
            int waitingSecs = ms.min * 60 + ms.sec;
            if(CurrentWave > 0) {
                Time prev_ms = waves[CurrentWave - 1].invokeTime;
                waitingSecs -= prev_ms.min * 60 + prev_ms.sec;
            }
            innerCoroutine = Waiting(waitingSecs);
            StartCoroutine(innerCoroutine);

            yield return new WaitForSeconds(waitingSecs);

            foreach(Set set in wave.sets) {
                GenerateUnit(set);
                yield return new WaitForSeconds(0.3f);
            }
            Debug.Log(CurrentWave);
            CurrentWave++;
        }
    }

    IEnumerator Waiting(int waitingSecs) {
        float time = waitingSecs;
        while (time > 0) {
            time -= 1;
            //Debug.Log("적 영웅소환까지 남은 시간 : " + time);
            yield return new WaitForSeconds(1.0f);
        }
        yield return 0;
    }

    public void HeroReturn(string id) {
        List<GameObject> clone = new List<GameObject>();
        foreach(GameObject obj in generatedHeroes) {
            if(obj != null) {
                clone.Add(obj);
            }
            clone.Add(obj);
        }
        foreach(GameObject obj in clone) {
            if(obj == null) {
                generatedHeroes.Remove(obj);
            }
        }
        GameObject hero = generatedHeroes.Find(x => x.name == "id");
        generatedHeroes.Remove(hero);
    }

    private void GenerateUnit(Set set) {
        if (generatedHeroes.Exists(x => x.name == set.id)) return;

        CardData card = AccountManager.Instance.GetUnitCardData(set.id);
        if (card == null) return;

        GameObject unit = Instantiate(set.prefab, transform);
        unit.name = set.id;

        UnitAI unitAI = unit.GetComponent<UnitAI>();
        unitAI.SetUnitData(card.unit, set.lv);
        unit.layer = LayerMask.NameToLayer("EnemyUnit");

        GameObject name = unit.transform.Find("Name").gameObject;
        name.SetActive(true);
        name.GetComponent<TextMeshPro>().text = card.unit.name;

        generatedHeroes.Add(unit);
    }

    [System.Serializable]
    public class Wave {
        public Time invokeTime;
        public List<Set> sets;
    }

    [System.Serializable]
    public class Time {
        public int min;
        public int sec;
    }

    [System.Serializable]
    public class Set {
        public GameObject prefab;
        public string id;
        public int lv;
    }
}
