using UnityEngine;
using UnityEngine.UI;

namespace ingameUIModules {
    public class CircularSlider : MonoBehaviour {
        public Image slider;
        public delegate void OnLoseTerritory();
        public delegate void OnOccupyTerritory();

        private OnLoseTerritory loseTerritory;
        private OnOccupyTerritory occupyTerritory;

        public void IncreaseByPercentage(float amount) {
            float val = amount / 100.0f;
            slider.fillAmount += val;

            if (slider.fillAmount == 1) occupyTerritory();
        }

        public void DecreaseByPercentage(float amount) {
            float val = amount / 100.0f;
            slider.fillAmount -= val;

            if (slider.fillAmount == 0) loseTerritory();
        }

        public void Reset() {
            slider.fillAmount = 0;
        }

        public void OccupyImmidiately() {
            IncreaseByPercentage(100);
        }

        public void LoseImmidiately() {
            DecreaseByPercentage(100);
        }
    }

}