using UnityEngine;

public class Heal : MonoBehaviour {
    public float time;
    public float delayTime = 1f;

    private void Update() {
        time += Time.deltaTime;
        if (time >= delayTime) {
            healUnit();
            time = 0f;
        }
    }

    private void healUnit() {
        UnitAI unit = transform.gameObject.GetComponent<UnitAI>();
        if (unit == null) return;
        unit.Healed();
    }
}
