using UnityEngine;

public class Heal : MonoBehaviour {
    public float time = 0f;
    public float delayTime;

    private void Update() {
        time += Time.deltaTime;
        if (time >= delayTime) {
            healUnit();
            time = 0f;
        }
    }

    private void healUnit() {
        if (gameObject.layer != 10) return;
        UnitAI unit = transform.gameObject.GetComponent<UnitAI>();
        if (unit == null) return;
        unit.Healed();
    }
}
