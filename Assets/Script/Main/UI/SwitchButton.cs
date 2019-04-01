using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using UniRx;
using UniRx.Triggers;

public class SwitchButton : MonoBehaviour {

    public bool switchOn { get; set; }

    private void Awake() {
        switchOn = true;
    }

    private void Start() {
        GetComponent<Button>().onClick.AsObservable().Subscribe(_ => SwitchActive());
    }
    public void SwitchActive() {
        switchOn = !GetComponent<Animator>().GetBool("OnSwitch");
        GetComponent<Animator>().SetBool("OnSwitch", switchOn);
    }
}
