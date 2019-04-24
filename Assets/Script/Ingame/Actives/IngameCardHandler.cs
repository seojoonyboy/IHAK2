using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class IngameCardHandler : MonoBehaviour {
    protected GameObject select;

    private GameObject temp;
    void Start() {
        Init();
    }

    public void Init() {
        var click = GetComponent<Toggle>()
            .OnPointerDownAsObservable();

        var stream = click
            .Buffer(click.Throttle(System.TimeSpan.FromMilliseconds(200)))
            .Where(x => x.Count >= 2)
            .Subscribe(_ => {
                OnDoubleClick();
            });

        var singleStream = click
            .Buffer(click.Throttle(System.TimeSpan.FromMilliseconds(200)))
            .Where(x => x.Count == 1)
            .Subscribe(_ => {
                OnSingleClick();
            });
    }

    protected virtual void OnDoubleClick() {
        Debug.Log("더블 클릭");
    }

    protected virtual void OnSingleClick() {
        Debug.Log("싱글 클릭");
    }
}
