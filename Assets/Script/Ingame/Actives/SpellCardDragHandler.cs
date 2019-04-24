using UnityEngine;
using UnityEngine.EventSystems;

public class SpellCardDragHandler : MonoBehaviour {
    [SerializeField] protected Camera camera;
    [SerializeField] protected bool isInit = false;
    [SerializeField] protected GameObject parentBuilding;
    [SerializeField] public string[] data;
    [SerializeField] protected int coolTime;
    [SerializeField] protected IngameDeckShuffler deckShuffler;
    [SerializeField] public GameObject targetCard;

    private bool _mouseState;
    public GameObject Target;
    public Vector3 screenSpace;
    public Vector3 offset;

    void Awake() {
        Target = gameObject;
    }

    public virtual void OnBeginDrag() {
        Debug.Log("드래그 시작");
    }

    public virtual void OnEndDrag() {
        Debug.Log("드래그 종료");
    }

    void Update() {
        // Debug.Log(_mouseState);
        if (Input.GetMouseButtonDown(0)) {
            if (Target == GetClickedObject()) {
                _mouseState = true;
                screenSpace = Camera.main.WorldToScreenPoint(Target.transform.position);
                offset = Target.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));

                OnBeginDrag();
            }
        }
        if (Input.GetMouseButtonUp(0)) {

            OnEndDrag();
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

    GameObject GetClickedObject() {
        GameObject target = null;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        LayerMask mask = 1 << LayerMask.NameToLayer("UI");
        RaycastHit2D hits = Physics2D.Raycast(new Vector2(mousePos.x, mousePos.y), Vector2.zero, Mathf.Infinity, mask);
        if(hits.collider == null) return null;
        target = hits.collider.gameObject;
        Debug.Log(target.name);
        return target;
    }

    public virtual void OnDrag() {
        Debug.Log("드래그!!!");
    }

    public virtual void Init(Camera camera, GameObject parentBuilding, IngameDeckShuffler deckShuffler, string[] data, int coolTime, GameObject targetCard) {
        this.camera = camera;
        this.parentBuilding = parentBuilding;
        this.deckShuffler = deckShuffler;
        this.data = data;
        this.coolTime = coolTime;
        this.targetCard = targetCard;

        isInit = true;

        int range;
        int.TryParse(this.data[0], out range);
        range /= 2;

        GetComponent<CircleCollider2D>().radius = range;
        transform.GetChild(0).localScale *= range;
    }
}
