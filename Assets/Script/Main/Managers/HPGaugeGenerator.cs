using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPGaugeGenerator : MonoBehaviour {

    public GameObject tileGroup;

    public GameObject HpGauge;
    public Sprite player;
    public Sprite enemy;

    private void Start() {
        int count = tileGroup.transform.childCount - 1;

        for(int i = 0; i< count; i++) {
            GameObject building = tileGroup.transform.GetChild(i).GetChild(0).gameObject;
            Destroy(building.transform.GetChild(0).gameObject);
            GameObject gauge = Instantiate(HpGauge, building.transform);
            gauge.transform.SetAsFirstSibling();
            if (building.layer == 9) {
                gauge.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = enemy;
            }
        }         
    }

}
