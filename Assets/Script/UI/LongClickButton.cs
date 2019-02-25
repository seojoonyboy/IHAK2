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
    // Update is called once pe r frame

    private void Update() {
        if (pointerDown && pointerInside) {
            if (pointerDownTimer <= requiredHoldTime)
                pointerDownTimer += Time.deltaTime;

            if (pointerDownTimer >= requiredHoldTime && onLongClick != null) {
                onLongClick.Invoke();
            }
        }
    }


    public void OnPointerDown(PointerEventData eventData) {
        pointerDown = true;
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
        pointerInside = true;
    }

    public void OnPointerExit(PointerEventData pointerEventData) {        
        pointerInside = false;
        pointerDownTimer = 0;
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
