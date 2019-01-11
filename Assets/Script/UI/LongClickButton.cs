using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LongClickButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    private bool pointerDown;
    private float pointerDownTimer;

    [SerializeField]
    private float requiredHoldTime;

    public UnityEvent onLongClick;
    public UnityEvent onPointerUp;
    public UnityEvent onShortClick;
    // Update is called once per frame
    void Update() {
        if (pointerDown) {
            pointerDownTimer += Time.deltaTime;
            if (pointerDownTimer >= requiredHoldTime && onLongClick != null) {
                onLongClick.Invoke();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        pointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData) {
        if(pointerDownTimer < requiredHoldTime && onShortClick != null) {
            onShortClick.Invoke();
        }
        onPointerUp.Invoke();
        Reset();
    }

    private void Reset() {
        pointerDown = false;
        pointerDownTimer = 0;
    }
}
