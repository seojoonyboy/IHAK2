using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal_detector : MonoBehaviour {
    List<GameObject> targets = new List<GameObject>();

    public int healAmount = 0;
    public int range = 0;

    void OnTriggerStay2D(Collider2D collision) {
        //Debug.Log(collision.gameObject.name + "감지됨");
        if (collision.gameObject.layer == 10) {
            if(!targets.Exists(x => x == collision.gameObject)) targets.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == 10) {
            targets.Remove(collision.gameObject);
        }
    }

    public void ActivateSpell() {
        foreach (GameObject obj in targets) {
            UnitAI unitAI = obj.GetComponent<UnitAI>();
            if (unitAI != null) {
                IngameAlarm.instance.SetAlarm("체력 회복 +" + healAmount);
                unitAI.health += healAmount;
            }
        }
        Destroy(gameObject);
    }

    public void Init(DataModules.Skill skill) {
        string[] args = skill.method.args.Split(',');
        int val = 0;
        int.TryParse(args[0], out val);
        range = val * 10;

        int.TryParse(args[1], out healAmount);
        GetComponent<CircleCollider2D>().radius = range;
        int.TryParse(args[1], out healAmount);
    }
}
