using BitBenderGames;
using DataModules;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpellCardDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
    [SerializeField] public GameObject prefab, obj;
    [SerializeField] Camera camera;
    [SerializeField] protected bool isInit = false;
    [SerializeField] protected GameObject targetCard;
    [SerializeField] protected string[] data;
    [SerializeField] protected int coolTime;
    [SerializeField] protected IngameDeckShuffler deckShuffler;

    protected Vector3 startScale;
    protected Vector3 startPosition;

    void Start() {
        MoveBlock();
    }

    public virtual void OnBeginDrag(PointerEventData eventData) {
        startPosition = transform.position;
        startScale = transform.localScale;

        Camera.main.GetComponent<MobileTouchCamera>().enabled = false;
    }

    public virtual void OnEndDrag(PointerEventData eventData) {
        transform.position = startPosition;

        Camera.main.GetComponent<MobileTouchCamera>().enabled = true;
    }

    public virtual void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;

        if (!isInit) {
            Debug.LogError("Prefab 관련 초기화가 정상적으로 되지 않았습니다!");
            return;
        }
        if (obj != null) {
            obj.SetActive(true);
            Vector3 origin = camera.ScreenToWorldPoint(Input.mousePosition);
            obj.transform.position = new Vector3(origin.x, origin.y, 0);
        }
    }

    public virtual void Init(ActiveSkill skill, GameObject targetCard) {
        obj = Instantiate(prefab, GameObject.Find("Map").transform);
        data = skill.method.args;

        startPosition = transform.position;
        startScale = transform.localScale;
        deckShuffler = PlayerController.Instance.deckShuffler();
        camera = Camera.main;

        this.targetCard = targetCard;
        coolTime = skill.coolTime;

        isInit = true;
    }

    public bool UseCard() {
        transform.position = startPosition;
        transform.localScale = new Vector3(1, 1, 1);

        var result = deckShuffler.UseCard(gameObject);
        return result;
    }

    protected virtual void MoveBlock() {
        EventTrigger et = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;

        entry.callback.AddListener(
            (eventData) => Camera.main.GetComponent<TouchInputController>().OnEventTriggerPointerDown(null)
        );

        et.triggers.Add(entry);
    }
}
