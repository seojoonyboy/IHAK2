using System;
using System.Collections;
using System.Collections.Generic;
using DataModules;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class IngameCityManager : MonoBehaviour {
    [System.Serializable]
    public class BuildingInfo {
        public int tileNum;
        public bool activate;
        public int hp;
        public int maxHp;
        public Card cardInfo;
        public GameObject gameObject;

        public BuildingInfo() { }

        public BuildingInfo(int tileNum, bool activate, int hp, int maxHp, Card card, GameObject gameObject) {
            this.tileNum = tileNum;
            this.activate = activate;
            this.hp = hp;
            this.maxHp = maxHp;
            this.cardInfo = card;
            this.gameObject = gameObject;
        }
    }

    //현재 어떤 화면을 보고 있는지
    public int CurrentView;
    public ArrayList eachPlayersTileGroups = new ArrayList();

    public UpgradeInfo 
        hq_tier_1,
        hq_tier_2,
        hq_tier_3; 

    [SerializeField] private Image hpValueBar;
    [SerializeField] private Text hpValue;
    //[SerializeField] private Text maxHp;

    IngameSceneEventHandler ingameSceneEventHandler;
    public ProductResources productResources;

    private int cityHP = 0;
    private int cityMaxHP = 0;
    private Deck deck;
    public List<BuildingInfo> myBuildingsInfo = new List<BuildingInfo>();
    public List<BuildingInfo> enemyBuildingsInfo = new List<BuildingInfo>();

    private int[] demoTileIndex = { 6, 7, 8, 11, 12, 13, 16, 17, 18 };
    public int[] buildingList;

    void Awake() {
        ingameSceneEventHandler = IngameSceneEventHandler.Instance;
        ingameSceneEventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.TAKE_DAMAGE, TakeDamageEventOcccured);
    }

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

        for (int i = 0; i < demoTileIndex.Length; i++) {    // 3x3 마을용 연산
            BuildingInfo bi = new BuildingInfo();
            bi.tileNum = demoTileIndex[i];
            bi.activate = true;
            bi.gameObject = transform.GetChild(1).GetChild(demoTileIndex[i]).GetChild(0).gameObject;
            bi.cardInfo = transform.GetChild(1).GetChild(demoTileIndex[i]).GetChild(0).GetComponent<BuildingObject>().data.card;
            bi.hp = bi.maxHp = bi.cardInfo.hitPoint;
            cityHP += bi.hp;
            myBuildingsInfo.Add(bi);
        }

        cityMaxHP = cityHP;

        //maxHp.text = hpValue.text = cityMaxHP.ToString();
        hpValue.text = ((int)(cityHP / cityMaxHP) * 100).ToString() + "%";
        hpValueBar.fillAmount = cityHP / cityMaxHP;
        //InitProduction();

        productResources = transform.GetChild(1).GetComponent<TileGroup>().touchPerProdPower;


        TakeDamage(Target.ENEMY_1, 6, 20);
        TakeDamage(Target.ENEMY_1, 6, 80);
        TakeDamage(Target.ENEMY_1, 7, 100);
        //테스트
        //SkillDetail skillDetail = new SkillDetail();
        //skillDetail.id = 1;
        //skillDetail.methodName = "마그마 스킬";
        //skillDetail.args = "5,6,1,15";
        //gameObject.AddComponent<Temple_Damager>().GenerateAttack(skillDetail);
    }

    private void Update() {
        //if (Input.GetMouseButtonDown(0)) {
        //    Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    Ray2D ray = new Ray2D(worldPoint, Vector2.zero);
        //    RaycastHit2D hit = Physics2D.Raycast(ray.origin, worldPoint);
        //    if (hit.collider.gameObject.tag == "Building") {
        //        Debug.Log(hit.collider.gameObject.ToString());
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
        foreach (BuildingInfo bi in myBuildingsInfo) {
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

    private void TakeDamageEventOcccured(Enum Event_Type, Component Sender, object Param) {
        object[] parms = (object[])Param;
        Target target = (Target)parms[0];
        int[] targetTileNums = (int[])parms[1];
        int damageAmount = (int)parms[2];

        TakeDamage(
            target: target,
            numbers: targetTileNums.ToList(),
            amount: damageAmount
        );
    }

    public bool TakeDamage(Target target, int tileNum, int amount) {
        switch (target) {
            case Target.ENEMY_1:
                BuildingInfo enemyBuilding = enemyBuildingsInfo.Find(x => x.tileNum == tileNum);
                if (enemyBuilding == null) return false;
                enemyBuilding.hp -= amount;
                if (enemyBuilding.hp < enemyBuilding.maxHp) {
                    enemyBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(true); // 건물 하위에 있는 체력게이지 활성화.
                    float hp = enemyBuilding.hp;
                    float maxHp = enemyBuilding.maxHp;
                    float hpScaleX = hp / maxHp;
                    //Debug.Log(hpScaleX);
                    enemyBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(hpScaleX, 1, 1);
                }
                if (enemyBuilding.hp < 0) BuildingDestroyed(enemyBuilding);
                break;
            case Target.ME:
                BuildingInfo myBuilding = myBuildingsInfo.Find(x => x.tileNum == tileNum);
                if (myBuilding == null) return false;
                myBuilding.hp -= amount;
                if (myBuilding.hp < myBuilding.maxHp) {
                    myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(true); // 건물 하위에 있는 체력게이지 활성화.
                    float hp = myBuilding.hp;
                    float maxHp = myBuilding.maxHp;
                    float hpScaleX = hp / maxHp;
                    Debug.Log(hpScaleX);
                    myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(hpScaleX, 1, 1);
                }

                if (myBuilding.hp < 0) BuildingDestroyed(myBuilding);
                break;
        }
        return true;
    }

    public bool TakeDamage(Target target, List<int> numbers, int amount) {
        switch (target) {
            case Target.ENEMY_1:
                foreach (int tileNum in numbers) {
                    //Debug.Log(tileNum + "에 데미지");
                    BuildingInfo enemyBuilding = enemyBuildingsInfo.Find(x => x.tileNum == tileNum);
                    if (enemyBuilding == null) return false;
                    enemyBuilding.hp -= amount;
                    if(enemyBuilding.hp < enemyBuilding.maxHp) {
                        enemyBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(true); // 건물 하위에 있는 체력게이지 활성화.
                        float hp = enemyBuilding.hp;
                        float maxHp = enemyBuilding.maxHp;
                        float hpScaleX = hp / maxHp;
                        //Debug.Log(hpScaleX);
                        enemyBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(hpScaleX, 1, 1);
                    }
                    if (enemyBuilding.hp < 0) BuildingDestroyed(enemyBuilding);
                }
                break;
            case Target.ME:
                foreach (int tileNum in numbers) {
                    BuildingInfo myBuilding = myBuildingsInfo.Find(x => x.tileNum == tileNum);
                    if (myBuilding == null) return false;
                    myBuilding.hp -= amount;
                    if (myBuilding.hp < myBuilding.maxHp) {
                        myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(true); // 건물 하위에 있는 체력게이지 활성화.
                        float hp = myBuilding.hp;
                        float maxHp = myBuilding.maxHp;
                        float hpScaleX = hp / maxHp;
                        Debug.Log(hpScaleX);
                        myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(hpScaleX, 1, 1);
                    }
                    if (myBuilding.hp < 0) BuildingDestroyed(myBuilding);
                }
                break;
        }
        return true;
    }

    private void BuildingDestroyed(BuildingInfo buildingInfo) {
        buildingInfo.hp = 0;
        buildingInfo.activate = false;
    }

    public void SetEnemyBuildingLists(ref GameObject tilegroup) {
        foreach(Transform tile in tilegroup.transform) {
            if(tile.childCount == 1) {
                int tileNum = tile.GetComponent<TileObject>().tileNum;
                GameObject building = tile.GetChild(0).gameObject;
                BuildingObject buildingObject = building.GetComponent<BuildingObject>();
                Card card = buildingObject.data.card;

                BuildingInfo info = new BuildingInfo(
                    tileNum: tileNum,
                    activate: true,
                    hp: card.hitPoint,
                    maxHp: card.hitPoint,
                    card: card,
                    gameObject: building
                );

                enemyBuildingsInfo.Add(info);
            }
        }
    }

    public enum Target {
        ME,
        ENEMY_1
    }
}
