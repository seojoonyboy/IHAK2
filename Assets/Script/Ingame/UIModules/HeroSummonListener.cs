using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using System;
using System.Linq;
using DataModules;
using BitBenderGames;

namespace ingameUIModules {
    public class HeroSummonListener : MonoBehaviour {
        bool anyTogglesOn = false;

        void Start() {
            var clickStream = this.UpdateAsObservable().Where(_ => Input.GetMouseButtonUp(0));

            clickStream
                .Subscribe(_ => IsSummonOk());

            //clickStream
            //    .Buffer(clickStream.Throttle(TimeSpan.FromMilliseconds(200)))
            //    .Where(x => x.Count >= 2)
            //    .Subscribe(_ => Debug.Log("Double Click"));

            //OffListener();
        }

        private bool IsSummonOk() {
            if (!anyTogglesOn) return false;

            GraphicRaycaster m_Raycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
            if (m_Raycaster == null) return false;

            PointerEventData m_PointEventData = new PointerEventData(FindObjectOfType<EventSystem>());
            m_PointEventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            m_Raycaster.Raycast(m_PointEventData, results);

            if (results.Count == 0) {
                ToggleGroup tg = PlayerController.Instance.deckShuffler().heroCardParent.parent.GetComponent<ToggleGroup>();
                var toggles = tg.ActiveToggles();
                Toggle toggle = toggles.ToList().First();

                //spell card가 선택된 상태
                if (toggle.GetComponent<HeroCardHandler>() == null) return false;

                if(toggle.GetComponent<HeroCardHandler>().instantiatedUnitObj == null) {
                    PlayerController.Instance.deckShuffler().UseCard(toggle.gameObject);
                }
                else {
                    IngameAlarm.instance.SetAlarm("이미 소환한 유닛입니다!");
                }

                tg.SetAllTogglesOff();
                //PlayerController.Instance.HeroSummon(toggle.GetComponent<ActiveCardInfo>().data, toggle.gameObject);

                //Debug.Log("화면을 클릭했음");
                return true;
            }
            return false;
        }

        public void ToggleListener(bool anyTogglesOn) {
            PlayerController.Instance.GoldResourceFlick.SetActive(anyTogglesOn);
            Camera.main.GetComponent<MobileTouchCamera>().enabled = !anyTogglesOn;
            this.anyTogglesOn = anyTogglesOn;
            //PlayerController.Instance.CitizenResourceFlick.SetActive(isOn);
        }
    }
}
