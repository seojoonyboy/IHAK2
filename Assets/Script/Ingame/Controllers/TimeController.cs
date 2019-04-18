using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour {
    public float timer;
    public string ms;
    [SerializeField] Text uiText;

	void Update () {
        timer += Time.deltaTime;
        System.TimeSpan t = System.TimeSpan.FromSeconds(timer);
        //timerFormatted = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);

        ms = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        uiText.text = ms;
    }
}
