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
            var clickStream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));
            clickStream
                .RepeatUntilDestroy(gameObject)
                .Where(_ => IsSummonOk() && ClickSummonableArea())
                .Subscribe(_ => Summon());
        }

        private bool ClickSummonableArea() {
            if (Input.GetMouseButtonDown(0)) {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                LayerMask mask = 1 << LayerMask.NameToLayer("Node");
                RaycastHit2D hits = Physics2D.Raycast(new Vector2(mousePos.x, mousePos.y), Vector2.zero, Mathf.Infinity, mask);
                if (hits.collider != null && hits.collider.name == "S10") {
                    return true;
                }
            }
            return false;
        }

        private void Summon() {
            ToggleGroup tg = PlayerController.Instance.deckShuffler().heroCardParent.parent.GetComponent<ToggleGroup>();
            var toggles = tg.ActiveToggles();
            Toggle toggle = toggles.ToList().First();
            PlayerController.Instance.deckShuffler().UseCard(toggle.gameObject);

            tg.SetAllTogglesOff();
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
                if (toggle.GetComponent<HeroCardHandler>() == null) {
                    Invoke("OffMobileTouchCamer", 0.1f);
                    return false;
                }
                if(toggle.GetComponent<HeroCardHandler>().instantiatedUnitObj == null) {
                    return true;
                }
                else {
                    IngameAlarm.instance.SetAlarm("이미 소환한 유닛입니다!");
                    return false;
                }
            }
            return false;
        }

        public void ToggleListener(bool anyTogglesOn) {
            PlayerController.Instance.GoldResourceFlick.SetActive(anyTogglesOn);
            Camera.main.GetComponent<MobileTouchCamera>().enabled = !anyTogglesOn;
            this.anyTogglesOn = anyTogglesOn;
            //PlayerController.Instance.CitizenResourceFlick.SetActive(isOn);
        }

        void OffMobileTouchCamer() {
            Camera.main.GetComponent<MobileTouchCamera>().enabled = true;
            Debug.Log("!!");
        }
    }
}
