using BitBenderGames;
using UnityEngine;
using UnityEngine.EventSystems;

public class IngameActiveCardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    protected IngameSceneEventHandler eventHandler;

    protected Vector3 startScale;
    protected Vector3 startPosition;

    protected Camera cam;
    [SerializeField] protected IngameDeckShuffler deckShuffler;

    void Awake() {
        eventHandler = IngameSceneEventHandler.Instance;
    }

    void Start() {
        Init();
    }

    public virtual void Init() {
        MoveBlock();
        deckShuffler = PlayerController.Instance.deckShuffler();
    }

    protected virtual void MoveBlock() {
        cam = Camera.main;

        EventTrigger et = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;

        entry.callback.AddListener(
            (eventData) => cam.GetComponent<TouchInputController>().OnEventTriggerPointerDown(null)
        );

        et.triggers.Add(entry);
    }

    protected void CancelDrag() {
        GetComponent<MagmaDragHandler>().enabled = false;

        transform.position = startPosition;
        transform.localScale = new Vector3(1, 1, 1);

        GameObject deactive = transform.Find("Deactive").gameObject;
        deactive.SetActive(true);
    }

    public virtual void OnBeginDrag(PointerEventData eventData) {
        startPosition = transform.position;
        startScale = transform.localScale;

        GetComponentInChildren<BoundaryCamMove>().isDrag = true;
    }

    public virtual void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;
    }

    public virtual void OnEndDrag(PointerEventData eventData) {
        transform.position = startPosition;
        GetComponentInChildren<BoundaryCamMove>().isDrag = false;
    }

    public bool UseCard() {
        transform.position = startPosition;
        transform.localScale = new Vector3(1, 1, 1);

        var result = deckShuffler.UseCard(gameObject);
        return result;
    }
}
