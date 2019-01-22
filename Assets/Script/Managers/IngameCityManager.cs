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
    
    public UpgradeInfo 
        hq_tier_1,
        hq_tier_2,
        hq_tier_3; 

    [SerializeField] private Image hpValueBar;
    [SerializeField] private Text hpValue;
    [SerializeField] private Text maxHp;

    public ProductResources productResources;

    private int cityHP = 0;
    private int cityMaxHP = 0;
    private Deck deck;
    private List<BuildingsInfo> buildingsInfo = new List<BuildingsInfo>();
    public int[] buildingList;

    private int[] demoIndex = { 6, 7, 8, 11, 12, 13, 16, 17, 18 };

    // Use this for initialization
    void Start () {
        deck = AccountManager.Instance.decks[0];
        buildingList = deck.coordsSerial;
        //for (int i = 0; i < deck.coordsSerial.Length - 1; i++) {
        //    BuildingsInfo bi = new BuildingsInfo();
        //    bi.id = deck.coordsSerial[i];
        //    bi.activate = true;
        //    if (i != deck.coordsSerial.Length / 2) {
        //        bi.cardInfo = transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<BuildingObject>().data.card;
        //        bi.hp = bi.maxHp = bi.cardInfo.hitPoint;
        //        cityHP += bi.hp;
        //    }
        //    buildingsInfo.Add(bi);
        //}

        for (int i = 0; i < demoIndex.Length; i++) {    // 3x3 마을용 연산
            BuildingsInfo bi = new BuildingsInfo();
            bi.id = deck.coordsSerial[demoIndex[i]];
            bi.activate = true;
            bi.cardInfo = transform.GetChild(1).GetChild(demoIndex[i]).GetChild(0).GetComponent<BuildingObject>().data.card;
            bi.hp = bi.maxHp = bi.cardInfo.hitPoint;
            cityHP += bi.hp;
            buildingsInfo.Add(bi);
        }

        cityMaxHP = cityHP;
        
        maxHp.text = hpValue.text = cityMaxHP.ToString();
        hpValueBar.fillAmount = cityHP / cityMaxHP;
        //InitProduction();

        productResources = transform.GetChild(1).GetComponent<TileGroup>().touchPerProdPower;
    }

    private void Update() {
        //if (Input.GetMouseButtonDown(0)) {
        //    Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    Ray2D ray = new Ray2D(worldPoint, Vector2.zero);
        //    RaycastHit2D hit = Physics2D.Raycast(ray.origin, worldPoint);
        //    if (hit.collider.gameObject.tag == "Building") {
        //        cityHP -= 100;
        //        hpValue.text = cityHP.ToString();
        //        hpValueBar.fillAmount = (float)cityHP / (float)cityMaxHP;
        //    }
        //}
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

    private void InitProduction() {
        PlayerController pc = FindObjectOfType<PlayerController>();
        foreach (BuildingsInfo bi in buildingsInfo) {
            if (bi.cardInfo == null)
                continue;
            if (bi.cardInfo.type == "prod") {
                switch (bi.cardInfo.prodType) {
                    case "gold":
                        pc.pInfo.clickGold[0] += bi.cardInfo.product.gold;
                        pc.pInfo.clickGold[1] += bi.cardInfo.product.food;
                        pc.pInfo.clickGold[2] += bi.cardInfo.product.environment;
                        break;
                    case "food":
                        pc.pInfo.clickFood[0] += bi.cardInfo.product.gold;
                        pc.pInfo.clickFood[1] += bi.cardInfo.product.food;
                        pc.pInfo.clickFood[2] += bi.cardInfo.product.environment;
                        break;
                    case "env":
                        pc.pInfo.clickEnvironment[0] += bi.cardInfo.product.gold;
                        pc.pInfo.clickEnvironment[1] += bi.cardInfo.product.food;
                        pc.pInfo.clickEnvironment[2] += bi.cardInfo.product.environment;
                        break;
                    case "all":
                        break;
                }
            }
        }
    }
}
