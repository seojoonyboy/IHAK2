using DataModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 최종적으로 배율을 적용하기 전까지 유저가 조작하는 정보를 저장하는 용도
/// </summary>
public class IngameUpgradeStream : MonoBehaviour {
    Dictionary<string, int> newRequests;        //새로운 할당 요청 <타입, 포인트>
    [SerializeField] PlayerController playerController;
    [SerializeField] IngameCityManager icm;

    List<Magnification> magnifications = new List<Magnification>();

    int point;
    public int Point {
        get { return point; }
        set {
            point = value;
            playerController.point_val.text = point.ToString();
        }
    }

    public void Init() {
        newRequests = new Dictionary<string, int>();

        magnifications = icm.myBuildings_mags;
        Point = playerController.Point;
        Debug.Log("Point : " + Point);
        playerController.point_val.text = Point.ToString();
    }

    public void Add(string key) {
        if (!newRequests.ContainsKey(key)) {
            newRequests.Add(key, 1);
        }
        else {
            newRequests[key] += 1;
        }

        Point--;
        //Debug.Log(key + " : " + newRequests[key]);
    }

    public void Remove(string key) {
        if (newRequests[key] == 1) newRequests.Remove(key);
        else newRequests[key] -= 1;

        Point++;
    }

    public void OnCancelButtonClicked() {
        Modal.instantiate("저장하지 않고 나갈시, 할당된 포인트가 초기화됩니다.\n정말 나가시겠습니까?", Modal.Type.YESNO, () => Cancel());
    }

    //최종 취소 버튼
    public void Cancel() {
        playerController.CloseUpgradeModal();
    }

    public void OnCirmButtonClicked() {
        Modal.instantiate("포인트를 적용하시겠습니까?", Modal.Type.YESNO, () => Confirm());
    }

    //최종 저장 버튼
    public void Confirm() {
        var list = icm.myBuildings_mags;
        foreach (KeyValuePair<string, int> pair in newRequests) {
            var item = list.Find(x => x.key == pair.Key);
            if (item == null) return;
            item.current_mag += item.mag * pair.Value;
            item.current_point += pair.Value;
            for(int i=0; i<pair.Value; i++) {
                playerController.Point--;
            }
        }
        playerController.CloseUpgradeModal();
    }
}
