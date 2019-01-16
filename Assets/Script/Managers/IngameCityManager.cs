using System.Collections;
using System.Collections.Generic;
using DataModules;
using UnityEngine;
using UnityEngine.UI;

public class IngameCityManager : MonoBehaviour {

    class BuildingsInfo {
        public int id;
        public bool activate;
        public int hp;
        public int maxHp;
        public Card cardInfo;
    }

    [SerializeField]
    private Image hpValueBar;
    [SerializeField]
    private Text hpValue;
    [SerializeField]
    private Text maxHp;

    private int cityHP = 0;
    private int cityMaxHP = 0;
    private Deck deck;
    private List<BuildingsInfo> buildingsInfo = new List<BuildingsInfo>();
    public int[] buildingList;

    // Use this for initialization
    void Start () {
        deck = AccountManager.Instance.decks[0];
        buildingList = deck.coordsSerial;
        BuildingsInfo bi = new BuildingsInfo();
        for (int i = 0; i < deck.coordsSerial.Length - 1; i++) {
            bi.id = deck.coordsSerial[i];
            bi.activate = true;
            if (i != 12) {
                bi.cardInfo = transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<BuildingObject>().data.card;
                bi.hp = bi.maxHp = bi.cardInfo.hitPoint;
                cityHP += bi.hp;
            }
        }
        cityMaxHP = cityHP;
        
        maxHp.text = hpValue.text = cityMaxHP.ToString();
        hpValueBar.fillAmount = cityHP / cityMaxHP;
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Ray2D ray = new Ray2D(worldPoint, Vector2.zero);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, worldPoint);
            if (hit.collider.gameObject.tag == "Building") {
                cityHP -= 100;
                hpValue.text = cityHP.ToString();
                hpValueBar.fillAmount = (float)cityHP / (float)cityMaxHP;
            }
        }
    }

    public void OnCollisionEnter(Collision col) {
        Debug.Log(col.ToString());
    }
    public void OnCollisionEnter2D(Collision2D col) {
        Debug.Log(col.ToString());
        if(col.gameObject.tag == "Building") {
            cityHP -= 100;
            hpValue.text = cityHP.ToString();
            hpValueBar.fillAmount = cityHP / cityMaxHP;
        }
    }
}
