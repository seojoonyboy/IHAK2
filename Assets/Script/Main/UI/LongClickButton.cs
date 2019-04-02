using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LongClickButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,  IPointerDownHandler, IPointerUpHandler {
    private bool pointerDown;
    private bool pointerInside;
    [SerializeField]
    private float pointerDownTimer;

    [SerializeField]
    public float requiredHoldTime;

    public UnityEvent onLongClick;
    public UnityEvent onPointerUp;
    public UnityEvent onShortClick;

    [SerializeField]
    public Vector3 clickLocation;
    [SerializeField]
    public bool move = false;
    [SerializeField]
    public GameObject fillGauge;
    // Update is called once pe r frame

    private void Start() {
        fillGauge = DeckSettingController.Instance.radialfillGauge;
    }

    private void Update() {
        
        if (pointerDown && pointerInside) {

            if (clickLocation != Input.mousePosition) {
                move = true;
                fillGauge.SetActive(false);
            }
       

            if (move == false) {
                pointerDownTimer += Time.deltaTime;
                fillGauge.GetComponent<Image>().fillAmount = pointerDownTimer / requiredHoldTime;
            }

            if (pointerDownTimer <= requiredHoldTime && onShortClick != null) {
                onShortClick.Invoke();
            }

            if (move == false && pointerDownTimer >= requiredHoldTime && onLongClick != null) {
                onLongClick.Invoke();
                fillGauge.SetActive(false);
            }
        } 
    }


    public void OnPointerDown(PointerEventData eventData) {
        pointerDown = true;
        clickLocation = Input.mousePosition;
        fillGauge.SetActive(true);
        fillGauge.transform.position = clickLocation;
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
        pointerInside = true;
    }

    public void OnPointerExit(PointerEventData pointerEventData) {        
        pointerInside = false;
        pointerDownTimer = 0;
    }

    public void OnPointerUp(PointerEventData eventData) {
        /*
        if(pointerDownTimer >= requiredHoldTime && onLongClick != null) {
            onLongClick.Invoke();
        }*/
        onPointerUp.Invoke();
        Reset();
    }

    private void Reset() {
        pointerDown = false;
        pointerDownTimer = 0;
        clickLocation = Vector3.zero;
        move = false;
        fillGauge.SetActive(false);
        fillGauge.GetComponent<Image>().fillAmount = 0f;
    }
}
