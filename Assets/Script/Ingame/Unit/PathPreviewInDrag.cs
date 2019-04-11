using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IngameModule {
    public class PathPreviewInDrag : MonoBehaviour {
        GraphicRaycaster m_Raycaster;
        PointerEventData m_PointEventData;

        float boxSize = 100.0f;

        IEnumerator coroutine;
        private bool canSearch = false;

        LayerMask mask;

        void Awake() {
            m_Raycaster = GetComponentInParent<GraphicRaycaster>();
            m_PointEventData = new PointerEventData(FindObjectOfType<EventSystem>());
            mask = LayerMask.GetMask("Node");
        }

        void Start() {
            coroutine = Serach();
            StartCoroutine(coroutine);
        }

        public void OnDrag(PointerEventData eventData) {
            canSearch = true;
        }

        IEnumerator Serach() {
            while (true) {
                if (canSearch) {
                    Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D hit = Physics2D.BoxCast(point, new Vector2(10, 10), 0, Vector3.forward, Mathf.Infinity, layerMask: mask);
                    Debug.DrawLine(point, new Vector3(point.x, point.y, -point.z * 100), Color.red);
                    if (hit.collider != null) {
                        Debug.Log(hit.transform.gameObject.name);
                    }
                }
                yield return new WaitForSeconds(1.0f);
            }
        }

        public void OnEndDrag(PointerEventData eventData) {
            canSearch = false;
        }

        void OnDestroy() {
            if(coroutine != null) {
                StopCoroutine(coroutine);
            }
        }

        void OnDrawGizmos() {
            Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(point, new Vector2(10, 10));
        }
    }
}