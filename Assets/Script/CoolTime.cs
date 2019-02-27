using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolTime : MonoBehaviour {
    public float coolTime;
    public float currTime;
    IEnumerator coroutine;

    public void StartCool() {
        currTime = 0;
        coroutine = Cool(coolTime);
        StartCoroutine(coroutine);
    }

    IEnumerator Cool(float coolTime) {
        while(currTime < coolTime) {
            currTime += 1;
            Work();
            Debug.Log("Cool 남은 시간 " + (coolTime - currTime) + "초");
            yield return new WaitForSeconds(1.0f);
        }
        OnTime();
    }

    void OnDestroy() {
        StopCoroutine(coroutine);
    }

    public virtual void Work() { }
    public virtual void OnTime() { Debug.Log("Cool 참"); }
}
