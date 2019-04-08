using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryCamMove : MonoBehaviour {
    public int Boundary;
    public int Speed;

    private int ScreenWidth;
    private int ScreenHeight;

    private GameObject cam;
    public bool isDrag = false;

    // Use this for initialization
    void Start() {
        ScreenWidth = Screen.width;
        ScreenHeight = Screen.height;

        cam = PlayerController.Instance.cam;
    }

    // Update is called once per frame
    void Update() {
        if (!isDrag) return;

        if (Input.mousePosition.x > ScreenWidth - Boundary) {
            cam.transform.position = new Vector3(
                cam.transform.position.x + Speed * Time.deltaTime,
                cam.transform.position.y,
                cam.transform.position.z
            );
        }

        if (Input.mousePosition.x < 0 + Boundary) {
            cam.transform.position = new Vector3(
                cam.transform.position.x - Speed * Time.deltaTime,
                cam.transform.position.y,
                cam.transform.position.z
            );
        }

        if (Input.mousePosition.y > ScreenHeight - Boundary) {
            cam.transform.position = new Vector3(
                cam.transform.position.x,
                cam.transform.position.y + Speed * Time.deltaTime,
                cam.transform.position.z
            );
        }

        if (Input.mousePosition.y < 0 + Boundary) {
            cam.transform.position = new Vector3(
                cam.transform.position.x,
                cam.transform.position.y - Speed * Time.deltaTime,
                cam.transform.position.z
            );
        }
    }
}
