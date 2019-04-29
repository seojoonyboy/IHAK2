using UnityEngine;

public class BoundaryCamMove : MonoBehaviour {
    [Range(0, 100)]
    public int LeftOffset;
    [Range(0, 100)]
    public int RightOffset;
    [Range(0, 100)]
    public int TopOffset;
    [Range(0, 100)]
    public int BottomOffset;

    public int Speed;

    private int ScreenWidth;
    private int ScreenHeight;

    private GameObject cam;
    public bool isDrag = false;

    // Use this for initialization
    void Start() {
        ScreenWidth = Screen.width;
        ScreenHeight = Screen.height;

        cam = Camera.main.gameObject;
    }

    // Update is called once per frame
    void Update() {
        if (!isDrag) return;

        if (Input.mousePosition.x > ScreenWidth - (ScreenWidth * (RightOffset / 100.0f))) {
            cam.transform.position = new Vector3(
                cam.transform.position.x + Speed * Time.deltaTime,
                cam.transform.position.y,
                cam.transform.position.z
            );
        }

        if (Input.mousePosition.x < 0 + (ScreenWidth * (LeftOffset / 100.0f))) {
            cam.transform.position = new Vector3(
                cam.transform.position.x - Speed * Time.deltaTime,
                cam.transform.position.y,
                cam.transform.position.z
            );
        }

        if (Input.mousePosition.y > ScreenHeight - (ScreenWidth * (TopOffset / 100.0f))) {
            cam.transform.position = new Vector3(
                cam.transform.position.x,
                cam.transform.position.y + Speed * Time.deltaTime,
                cam.transform.position.z
            );
        }

        if (Input.mousePosition.y < 0 + (ScreenWidth * (BottomOffset / 100.0f))) {
            cam.transform.position = new Vector3(
                cam.transform.position.x,
                cam.transform.position.y - Speed * Time.deltaTime,
                cam.transform.position.z
            );
        }
    }
}
