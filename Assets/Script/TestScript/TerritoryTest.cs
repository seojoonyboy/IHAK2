using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryTest : MonoBehaviour {
    [SerializeField] Camera[] cams;
    [SerializeField] Transform moveBtns;


    int nowCam;
    bool moved = false;

    // Use this for initialization
    void Start() {
        nowCam = 0;
        moveBtns.GetChild(nowCam).gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update() {

    }

    public void SwitchCam(int num) {
        cams[nowCam].transform.localPosition = new Vector3(0, 0, 0);
        cams[nowCam].enabled = false;
        moveBtns.GetChild(nowCam).gameObject.SetActive(true);
        nowCam = num;
        cams[nowCam].enabled = true;
        moveBtns.GetChild(nowCam).gameObject.SetActive(false);
        moved = true;
        RevertBtn(nowCam);
    }

    public void MoveCam(int num) {
        switch (num) {
            case 0:
                iTween.MoveTo(cams[nowCam].gameObject, iTween.Hash("x", cams[nowCam].transform.position.x - 200, "time", 0.3f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
                break;
            case 1:
                iTween.MoveTo(cams[nowCam].gameObject, iTween.Hash("x", cams[nowCam].transform.position.x + 200, "time", 0.3f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
                break;
            case 2:
                iTween.MoveTo(cams[nowCam].gameObject, iTween.Hash("y", cams[nowCam].transform.position.y + 320, "time", 0.3f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
                break;
            case 3:
                iTween.MoveTo(cams[nowCam].gameObject, iTween.Hash("y", cams[nowCam].transform.position.y - 320, "time", 0.3f, "delay", 0, "easetype", iTween.EaseType.easeInOutQuart));
                break;
        }
        RevertBtn(num);
    }

    private void RevertBtn(int num) {
        moved = !moved;
        if (moved) {
            for (int i = 0; i < 4; i++) {
                moveBtns.GetChild(i).gameObject.SetActive(false);
            }
            switch (num) {
                case 0:
                    moveBtns.GetChild(1).gameObject.SetActive(true);
                    break;
                case 1:
                    moveBtns.GetChild(0).gameObject.SetActive(true);
                    break;
                case 2:
                    moveBtns.GetChild(3).gameObject.SetActive(true);
                    break;
                case 3:
                    moveBtns.GetChild(2).gameObject.SetActive(true);
                    break;
            }
        }
        else {
            for (int i = 0; i < 4; i++) {
                moveBtns.GetChild(i).gameObject.SetActive(true);
            }
            moveBtns.GetChild(nowCam).gameObject.SetActive(false);
        }
    }
}
