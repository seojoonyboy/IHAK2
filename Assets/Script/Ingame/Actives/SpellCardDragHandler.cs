using UnityEngine;
using UnityEngine.EventSystems;

public class SpellCardDragHandler : MonoBehaviour {
    [SerializeField] protected Camera camera;
    [SerializeField] protected bool isInit = false;
    [SerializeField] protected GameObject parentBuilding;
    [SerializeField] protected string[] data;
    [SerializeField] protected int coolTime;
    [SerializeField] protected IngameDeckShuffler deckShuffler;

    private bool _mouseState;
    public GameObject Target;
    public Vector3 screenSpace;
    public Vector3 offset;

    void Awake() {
        Target = gameObject;
    }

    public virtual void OnBeginDrag() { }

    public virtual void OnEndDrag() { }

    void Update() {
        // Debug.Log(_mouseState);
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hitInfo;
            if (Target == GetClickedObject(out hitInfo)) {
                _mouseState = true;
                screenSpace = Camera.main.WorldToScreenPoint(Target.transform.position);
                offset = Target.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));
            }
        }
        if (Input.GetMouseButtonUp(0)) {
            _mouseState = false;
        }
        if (_mouseState) {
            //keep track of the mouse position
            var curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);

            //convert the screen mouse position to world point and adjust with offset
            var curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) + offset;

            //update the position of the object in the world
            Target.transform.position = curPosition;
        }
    }

    GameObject GetClickedObject(out RaycastHit hit) {
        GameObject target = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction * 100000, out hit)) {
            target = hit.collider.gameObject;
            Debug.Log(hit.collider.gameObject.name);
        }
        //Debug.Log(target.name);
        return target;
    }

    public virtual void OnDrag() {
        transform.position = Input.mousePosition;
        Debug.Log("드래그!!!");
    }

    public virtual void Init(Camera camera, GameObject parentBuilding, IngameDeckShuffler deckShuffler, string[] data, int coolTime) {
        this.camera = camera;
        this.parentBuilding = parentBuilding;
        this.deckShuffler = deckShuffler;
        this.data = data;
        this.coolTime = coolTime;
        isInit = true;

        int range;
        int.TryParse(this.data[0], out range);
        range /= 2;

        GetComponent<CircleCollider2D>().radius = range;
        transform.GetChild(0).localScale *= range;
    }
}
