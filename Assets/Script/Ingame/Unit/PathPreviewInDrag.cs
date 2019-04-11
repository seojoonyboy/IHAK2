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
        MapStation hq_mapStation;

        [SerializeField] GameObject arrowPref;
        GameObject prevGameObject;

        List<Vector3> path = new List<Vector3>();

        void Awake() {
            m_Raycaster = GetComponentInParent<GraphicRaycaster>();
            m_PointEventData = new PointerEventData(FindObjectOfType<EventSystem>());
            mask = LayerMask.GetMask("Node");
        }

        void Start() {
            coroutine = Serach();
            StartCoroutine(coroutine);

            hq_mapStation = PlayerController.Instance.hq_mapStation;
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
                        if(prevGameObject != hit.transform.gameObject) {
                            ClearPrevPath();
                            //Debug.Log(hit.transform.gameObject.name);
                            path = PathFind(hit.transform.gameObject);

                            int arrowNum = path.Count - 1;
                            if(arrowNum >= 1) {
                                for (int i = 0; i < arrowNum; i++) {
                                    GameObject arrow = Instantiate(arrowPref, PlayerController.Instance.pathPrefabsParent);
                                    arrow.transform.position = path[i];

                                    var dir = path[i + 1] - path[i];
                                    var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                                    arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                                    float dist = Vector2.Distance(path[i + 1], path[i]);
                                    arrow.GetComponent<SpriteRenderer>().size = new Vector2(dist, 10);
                                }
                            }
                            prevGameObject = hit.transform.gameObject;
                        }
                    }
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        public void OnEndDrag(PointerEventData eventData) {
            canSearch = false;
            ClearPrevPath();

            transform.parent.GetComponent<HeroCardDragHandler>().path = path;
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

        List<Vector3> PathFind(GameObject collision) {
            MapStation station = collision.GetComponent<MapStation>();
            List<Vector3> path = new List<Vector3>();
            if(station != null) {
                path = MapNode.SearchPosition(
                    hq_mapStation,
                    station.mapPostion
                );
                return path;
            }

            MapRoad mapRoad = collision.GetComponent<MapRoad>();
            if(mapRoad != null) {
                path = MapNode.SearchPosition(
                    hq_mapStation, 
                    mapRoad.mapPostion
                );
                return path;
            }

            return path;
        }

        void ClearPrevPath() {
            foreach(Transform item in PlayerController.Instance.pathPrefabsParent) {
                Destroy(item.gameObject);
            }
        }
    }
}