using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class IngameCardHandler : MonoBehaviour {
    protected GameObject select;

    private GameObject temp;
    void Start() {
        Init();
    }

    public void Init() {
        var stream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));

        stream
            .Where(_ => isDetectObject() == true)
            .Subscribe(_ => SetDetectObject());

        stream
            .Where(_ => isSameObjectDetect() == true)
            .Buffer(stream.Throttle(System.TimeSpan.FromMilliseconds(200)))
            .Where(x => x.Count >= 2)
            .Subscribe(_ => {
                OnDoubleClick();
                select = null;
            });
    }

    private bool isDetectObject() {
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(position, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null) {
            Debug.Log(hit.collider.name);
            temp = hit.collider.gameObject;
            return true;
        }
        return false;
    }

    private void SetDetectObject() {
        select = temp;
    }

    private bool isSameObjectDetect() {
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(position, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && select == hit.collider.gameObject) {
            return true;
        }
        return false;
    }

    public void Detect() {
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(position, Vector2.zero);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null) {
            select = hit.collider.gameObject;
        }
    }

    protected virtual void OnDoubleClick() {
        Debug.Log("더블 클릭");
    }

    //public void _temp() {
    //    var stream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));
    //    var selectStream = stream.Where(_ => select == null).Subscribe(_ => select = _Detect()).AddTo(this);

    //    stream.Buffer(stream.Throttle(System.TimeSpan.FromMilliseconds(200))).Where(x => x.Count >= 2).Where(_ => select == Detect()).Subscribe(_ => { Debug.Log("더블!"); select = null; });
    //    stream.Buffer(stream.Throttle(System.TimeSpan.FromMilliseconds(200))).Where(x => x.Count >= 2).Subscribe(_ => select = null);
    //    stream.Where(_ => select != null).Buffer(stream.Throttle(System.TimeSpan.FromMilliseconds(200))).Where(x => x.Count < 2).Subscribe(_ => { Debug.Log("시간초과!"); select = null; });
    //}

    //public GameObject _Detect() {
    //    Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //    Ray2D ray = new Ray2D(position, Vector2.zero);
    //    RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

    //    if (hit.collider != null) {
    //        return hit.collider.gameObject;
    //    }
    //    return null;
    //}
}
