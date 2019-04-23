using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using System;

namespace ingameUIModules {
    public class HeroSummonListener : MonoBehaviour {
        void Start() {
            var clickStream = this.UpdateAsObservable().Where(_ => Input.GetMouseButtonUp(0));

            clickStream
                .Subscribe(_ => IsSummonOk());

            clickStream
                .Buffer(clickStream.Throttle(TimeSpan.FromMilliseconds(200)))
                .Where(x => x.Count >= 2)
                .Subscribe(_ => Debug.Log("Double Click"));

            OffListener();
        }

        private bool IsSummonOk() {
            GraphicRaycaster m_Raycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
            if (m_Raycaster == null) return false;

            PointerEventData m_PointEventData = new PointerEventData(FindObjectOfType<EventSystem>());
            m_PointEventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            m_Raycaster.Raycast(m_PointEventData, results);

            if (results.Count == 0) {
                Debug.Log("화면을 클릭했음");
                return true;
            }
            return false;
        }

        public void OffListener() {
            GetComponent<ObservableUpdateTrigger>().enabled = false;
        }

        public void OnListener() {
            GetComponent<ObservableUpdateTrigger>().enabled = false;
        }

        public void ToggleListener(bool isOn) {
            GetComponent<ObservableUpdateTrigger>().enabled = isOn;
        }
    }
}
