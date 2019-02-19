using System;
using System.Collections;
using System.Collections.Generic;
using DataModules;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Spine.Unity;

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

    [Header(" - TotalHPUI")]
    [SerializeField] private Image hpValueBar;
    [SerializeField] private Text hpValue;
    [SerializeField] private GameObject enemyTotalHPGauge;
    [SerializeField] IngameSceneUIController ingameSceneUIController;

    [Space(10)]

    [Header(" - TotalHPInformation")]
    [SerializeField] private int enemyTotalHP;
    [SerializeField] public int enemyCurrentTotalHP;
    [SerializeField] public int cityHP = 0;
    [SerializeField] private int cityMaxHP = 0;

    [Space(10)]

    [Header(" - ProductResource")]
    public ProductResources productResources;
    public ProductResources unActiveResources;

    [Space(10)]

    [Header(" - DeckInfo")]
    [SerializeField] private Deck deck;
    public List<BuildingInfo> myBuildingsInfo = new List<BuildingInfo>();
    public List<BuildingInfo> enemyBuildingsInfo = new List<BuildingInfo>();

    [Space(10)]
    [Header(" - PlayerDeck")]
    private int[] demoTileIndex = { 6, 7, 8, 11, 12, 13, 16, 17, 18 };
    public int[] buildingList;


    [Space(10)]
    [Header(" - UnActive")]
    public GameObject unactiveImage;
    [SerializeField] GameObject unactiveGroup;
    public int unactiveBuildingIndex = 100;
    private bool unActiveAlert = false;

    [Space(10)]
    [Header(" - Other")]
    [SerializeField] private Sprite wreckSprite;
    [SerializeField] private SkeletonDataAsset wreckSpine;
    IngameSceneEventHandler ingameSceneEventHandler;
    IngameDeckShuffler ingameDeckShuffler;

    void Awake() {
        ingameSceneEventHandler = IngameSceneEventHandler.Instance;
        ingameSceneEventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.TAKE_DAMAGE, TakeDamageEventOcccured);
        ingameSceneEventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, OnHqUpgrade);
    }

    void OnDestroy() {
        ingameSceneEventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.TAKE_DAMAGE, TakeDamageEventOcccured);
        ingameSceneEventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.HQ_UPGRADE, OnHqUpgrade);
    }

    private void OnHqUpgrade(Enum Event_Type, Component Sender, object Param) {
        Debug.Log("HQ 업그레이트 이벤트 발생");
        ingameDeckShuffler.Clear();
        ingameDeckShuffler.InitUnitCard();
        ingameDeckShuffler.InitSkillCard();
    }

    // Use this for initialization
    void Start() {
        deck = AccountManager.Instance.decks[0];
        buildingList = deck.coordsSerial;
        ingameSceneUIController = FindObjectOfType<IngameSceneUIController>();
        wreckSpine.GetSkeletonData(false);
        ingameDeckShuffler = GetComponent<IngameDeckShuffler>();
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

        IEnumerable<GameObject> gameObjects =
            from x in myBuildingsInfo
            select x.gameObject;

        ingameSceneEventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, this, gameObjects.ToList());

        cityMaxHP = cityHP;

        //maxHp.text = hpValue.text = cityMaxHP.ToString();
        hpValue.text = ((int)(cityHP / cityMaxHP) * 100).ToString() + "%";
        hpValueBar.fillAmount = cityHP / cityMaxHP;
        //InitProduction();

        productResources = transform.GetChild(1).GetComponent<TileGroup>().touchPerProdPower;


        //TakeDamage(Target.ENEMY_1, 6, 20);
        //TakeDamage(Target.ENEMY_1, 6, 80);
        //RepairBuilding(Target.ENEMY_1, 6);
        //TakeDamage(Target.ENEMY_1, 7, 100);
        //RepairBuilding(Target.ENEMY_1, 7);
        //테스트
        //SkillDetail skillDetail = new SkillDetail();
        //skillDetail.id = 1;
        //skillDetail.methodName = "마그마 스킬";
        //skillDetail.args = "5,6,1,15";
        //gameObject.AddComponent<Temple_Damager>().GenerateAttack(skillDetail);
        //StartCoroutine("Damage");
        SetEnemyTotalHP();
        StartCoroutine("Repair");
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
        if (col.gameObject.tag == "Building") {
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
            if (bi.cardInfo.type == "prod" && bi.activate) {
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

    public bool RepairBuilding(Target target, int tileNum) { // 고정적으로 20%회복
        switch (target) {
            case Target.ENEMY_1:
                BuildingInfo enemyBuilding = enemyBuildingsInfo.Find(x => x.tileNum == tileNum);
                if (enemyBuilding == null) return false;
                if (enemyBuilding.activate == false) return false;
                if (enemyBuilding.hp >= enemyBuilding.maxHp) return false;

                //회복연산
                float enemyMaxHP = enemyBuilding.maxHp;
                int enemyAmount = Mathf.RoundToInt(enemyMaxHP * 0.2f);
                enemyBuilding.hp += enemyAmount;
                enemyCurrentTotalHP += enemyAmount; // 전체 체력의 회복;

                //전체체력에서 오버한 체력
                int plusHp;
                if (enemyCurrentTotalHP > enemyTotalHP) {
                    plusHp = enemyCurrentTotalHP - enemyTotalHP;
                    enemyCurrentTotalHP -= plusHp;
                }


                //회복뒤 건물 체력
                float enemyHp = enemyBuilding.hp;
                float enemyHpScaleX = enemyHp / enemyMaxHP;
                enemyBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(enemyHpScaleX, 1, 1);

                //회복뒤 전체 건물
                float totalHp = enemyCurrentTotalHP;
                float totalMaxHp = enemyTotalHP;
                float percent = totalHp / totalMaxHp;
                enemyTotalHPGauge.transform.parent.GetChild(2).GetChild(0).GetComponent<Text>().text = Mathf.RoundToInt(percent * 100f).ToString() + "%";
                enemyTotalHPGauge.GetComponent<Image>().fillAmount = percent;

                if (enemyBuilding.hp > enemyBuilding.maxHp) {
                    enemyBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(false); // 건물 하위에 있는 체력게이지 활성화.
                    enemyBuilding.hp = enemyBuilding.maxHp;
                    enemyBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(1, 1, 1);
                }

                if (enemyCurrentTotalHP > enemyTotalHP) {
                    enemyCurrentTotalHP = enemyTotalHP;
                    enemyTotalHPGauge.transform.parent.GetChild(2).GetChild(0).GetComponent<Text>().text = 100.ToString() + "%";
                    enemyTotalHPGauge.GetComponent<Image>().fillAmount = 1f;
                }



                if (enemyBuilding.hp < 0) BuildingDestroyed(enemyBuilding);
                break;
            case Target.ME:
                BuildingInfo myBuilding = myBuildingsInfo.Find(x => x.tileNum == tileNum);
                if (myBuilding == null) return false;
                if (myBuilding.activate == false) return false;

                float playerMaxHp = myBuilding.maxHp;
                int playerAmount = Mathf.RoundToInt(playerMaxHp * 0.2f);
                myBuilding.hp += playerAmount;
                float playerHp = myBuilding.hp;
                float playerHpScaleX = playerHp / playerMaxHp;
                myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(playerHpScaleX, 1, 1);


                if (myBuilding.hp > myBuilding.maxHp) {
                    myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(false); // 건물 하위에 있는 체력게이지 활성화.
                    myBuilding.hp = myBuilding.maxHp;
                    myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(1, 1, 1);
                }

                if (myBuilding.hp < 0) BuildingDestroyed(myBuilding);
                break;
        }
        return true;
    }




    public bool RepairBuilding(Target target, int tileNum, int amount) {
        switch (target) {
            case Target.ENEMY_1:
                BuildingInfo enemyBuilding = enemyBuildingsInfo.Find(x => x.tileNum == tileNum);
                if (enemyBuilding == null) return false;
                if (enemyBuilding.activate == false) return false;
                if (enemyBuilding.hp >= enemyBuilding.maxHp) return false;

                //회복연산
                float enemyMaxHP = enemyBuilding.maxHp;
                enemyBuilding.hp += amount;
                enemyCurrentTotalHP += amount;

                //오버 체력
                int plusHp;
                if (enemyCurrentTotalHP > enemyTotalHP) {
                    plusHp = enemyCurrentTotalHP - enemyTotalHP;
                    enemyCurrentTotalHP -= plusHp;
                }

                // 회복뒤 빌딩 체력게이지 연산
                float enemyHp = enemyBuilding.hp;
                float enemyHpScaleX = enemyHp / enemyMaxHP;
                enemyBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(enemyHpScaleX, 1, 1);

                // 회복뒤 전체 체력게이지 연산
                float totalHp = enemyCurrentTotalHP;
                float totalMaxHp = enemyTotalHP;
                float percent = totalHp / totalMaxHp;
                enemyTotalHPGauge.transform.parent.GetChild(2).GetChild(0).GetComponent<Text>().text = Mathf.RoundToInt(percent * 100f).ToString() + "%";
                enemyTotalHPGauge.GetComponent<Image>().fillAmount = percent;

                if (enemyBuilding.hp > enemyBuilding.maxHp) {
                    enemyBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(false); // 건물 하위에 있는 체력게이지 활성화.
                    enemyBuilding.hp = enemyBuilding.maxHp;
                    enemyBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(1, 1, 1);
                }

                if (enemyCurrentTotalHP > enemyTotalHP) {
                    enemyCurrentTotalHP = enemyTotalHP;
                    enemyTotalHPGauge.transform.parent.GetChild(2).GetChild(0).GetComponent<Text>().text = 100.ToString() + "%";
                    enemyTotalHPGauge.GetComponent<Image>().fillAmount = 1f;
                }

                if (enemyBuilding.hp < 0) BuildingDestroyed(enemyBuilding);

                break;

            case Target.ME:
                BuildingInfo myBuilding = myBuildingsInfo.Find(x => x.tileNum == tileNum);
                if (myBuilding == null) return false;
                if (myBuilding.activate == false) return false;

                float playerMaxHp = myBuilding.maxHp;
                myBuilding.hp += amount;
                float playerHp = myBuilding.hp;
                float playerHpScaleX = playerHp / playerMaxHp;
                myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(playerHpScaleX, 1, 1);

                if (myBuilding.hp > myBuilding.maxHp) {
                    myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(false); // 건물 하위에 있는 체력게이지 활성화.
                    myBuilding.hp = myBuilding.maxHp;
                    myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(1, 1, 1);
                }

                if (myBuilding.hp < 0) BuildingDestroyed(myBuilding);
                break;
        }
        return true;
    }

    public bool RepairBuilding(Target target, List<int> numbers, int amount) {
        switch (target) {
            case Target.ENEMY_1:
                foreach (int tileNum in numbers) {
                    //Debug.Log(tileNum + "에 데미지");
                    BuildingInfo enemyBuilding = enemyBuildingsInfo.Find(x => x.tileNum == tileNum);
                    if (enemyBuilding == null) return false;
                    enemyBuilding.hp += amount;
                    if (enemyBuilding.hp > enemyBuilding.maxHp) enemyBuilding.hp = enemyBuilding.maxHp;
                }
                break;
            case Target.ME:
                foreach (int tileNum in numbers) {
                    BuildingInfo myBuilding = myBuildingsInfo.Find(x => x.tileNum == tileNum);
                    if (myBuilding == null) return false;
                    myBuilding.hp += amount;
                    if (myBuilding.hp > 0) myBuilding.hp = myBuilding.maxHp;
                }
                break;
        }
        return true;
    }

    public bool RepairDestroyBuilding(Target target, int tileNum) { //체력 0되서 비활성화 된 체력.
        switch (target) {
            case Target.ENEMY_1:
                BuildingInfo enemyBuilding = enemyBuildingsInfo.Find(x => x.tileNum == tileNum);
                if (enemyBuilding == null) return false;
                if (enemyBuilding.activate == true) return false;
                if (enemyBuilding.gameObject.transform.parent.GetComponent<TileCollision>().check == true) return false;
                if (enemyBuilding.hp >= enemyBuilding.maxHp) return false;

                //회복 연산
                float enemyMaxHP = enemyBuilding.maxHp;
                int enemyAmount = Mathf.RoundToInt(enemyMaxHP * 0.5f);
                enemyBuilding.hp += enemyAmount;
                enemyCurrentTotalHP += enemyAmount;

                //체력게이지
                float enemyHp = enemyBuilding.hp;
                float enemyHpScaleX = enemyHp / enemyMaxHP;
                enemyBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(enemyHpScaleX, 1, 1);
                enemyBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                SetReviveImage(enemyBuilding.gameObject);
                enemyBuilding.activate = true;

                //전체체력게이지
                float totalHp = enemyCurrentTotalHP;
                float totalMaxHp = enemyTotalHP;
                float percent = totalHp / totalMaxHp;
                enemyTotalHPGauge.transform.parent.GetChild(2).GetChild(0).GetComponent<Text>().text = Mathf.RoundToInt(percent * 100f).ToString() + "%";
                enemyTotalHPGauge.GetComponent<Image>().fillAmount = percent;


                if (enemyBuilding.gameObject.GetComponent<BuildingObject>().data.card.id == "great_power_stone") {
                    GameObject detector = enemyBuilding.gameObject.transform.Find("Detector").gameObject;
                    if (detector != null) {
                        detector.GetComponent<Tower_Detactor>().enabled = true;
                    }
                }

                BuildingObject buildingObject = enemyBuilding.gameObject.GetComponent<BuildingObject>();
                string id = buildingObject.data.card.id;
                if (buildingObject.data.card.unit == null || string.IsNullOrEmpty(buildingObject.data.card.unit.name)) {
                    ingameDeckShuffler.ActivateCard(id, false, enemyBuilding.gameObject);
                }
                else {
                    ingameDeckShuffler.ActivateCard(id, true, enemyBuilding.gameObject);
                }

                if (enemyBuilding.hp > enemyBuilding.maxHp) {
                    enemyBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(false); // 건물 하위에 있는 체력게이지 활성화.
                    enemyBuilding.hp = enemyBuilding.maxHp;
                    enemyBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(1, 1, 1);
                }

                if (enemyCurrentTotalHP > enemyTotalHP) {
                    enemyCurrentTotalHP = enemyTotalHP;
                    enemyTotalHPGauge.transform.parent.GetChild(2).GetChild(0).GetComponent<Text>().text = 100.ToString() + "%";
                    enemyTotalHPGauge.GetComponent<Image>().fillAmount = 1f;
                }

                break;
        }
        return true;
    }



    public bool TakeDamage(Target target, int tileNum, int amount) {
        switch (target) {
            case Target.ENEMY_1:
                BuildingInfo enemyBuilding = enemyBuildingsInfo.Find(x => x.tileNum == tileNum);
                if (enemyBuilding == null) return false;
                if (enemyBuilding.hp <= 0) return false;

                //체력감소 연산
                enemyBuilding.hp -= amount;
                enemyCurrentTotalHP -= amount;

                //0에서 추가로 들어온 건물데미지만큼 전체 체력 회복;
                int minusHp;
                if (enemyBuilding.hp < 0) {
                    minusHp = 0 - enemyBuilding.hp;
                    enemyCurrentTotalHP += minusHp;
                }
                float enemyHp = enemyBuilding.hp;
                float enemyMaxHp = enemyBuilding.maxHp;

                //전체 체력게이지 연산
                float totalHp = enemyCurrentTotalHP;
                float totalMaxHp = enemyTotalHP;
                float percent = totalHp / totalMaxHp;
                enemyTotalHPGauge.transform.parent.GetChild(2).GetChild(0).GetComponent<Text>().text = Mathf.RoundToInt(percent * 100f).ToString() + "%";
                enemyTotalHPGauge.GetComponent<Image>().fillAmount = percent;

                if (enemyBuilding.hp < enemyBuilding.maxHp) {
                    enemyBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(true); // 건물 하위에 있는 체력게이지 활성화.
                    float hpScaleX = enemyHp / enemyMaxHp;
                    enemyBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(hpScaleX, 1, 1);
                }
                if (enemyBuilding.hp <= 0) {
                    float hpScaleX = enemyHp / enemyMaxHp;
                    enemyBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(0, 1, 1);
                    enemyBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    enemyBuilding.hp = 0;
                    BuildingDestroyed(enemyBuilding);
                }

                if (enemyCurrentTotalHP < 0) {
                    enemyTotalHPGauge.GetComponent<Image>().fillAmount = 0;
                    enemyCurrentTotalHP = 0;
                    enemyTotalHPGauge.transform.parent.GetChild(2).GetChild(0).GetComponent<Text>().text = 0.ToString() + "%";
                }
                IngameScoreManager.Instance.AddScore(amount, IngameScoreManager.ScoreType.Attack);
                break;
            case Target.ME:
                BuildingInfo myBuilding = myBuildingsInfo.Find(x => x.tileNum == tileNum);
                if (myBuilding == null) return false;
                myBuilding.hp -= amount;
                float playerHp = myBuilding.hp;
                float playerMaxHp = myBuilding.maxHp;
                if (myBuilding.hp < myBuilding.maxHp) {
                    myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(true); // 건물 하위에 있는 체력게이지 활성화.
                    float hpScaleX = playerHp / playerMaxHp;
                    myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(hpScaleX, 1, 1);
                }

                if (myBuilding.hp < 0) {
                    float hpScaleX = playerHp / playerMaxHp;
                    myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(0, 1, 1);
                    myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    BuildingDestroyed(myBuilding);
                }
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
                    if (enemyBuilding.hp <= 0) return false;
                    enemyBuilding.hp -= amount;
                    enemyCurrentTotalHP -= amount;
                    int minusHp;
                    if (enemyBuilding.hp < 0) {
                        minusHp = 0 - enemyBuilding.hp;
                        enemyCurrentTotalHP += minusHp;
                    }
                    float enemyHp = enemyBuilding.hp;
                    float enemyMaxHp = enemyBuilding.maxHp;

                    float totalHp = enemyCurrentTotalHP;
                    float totalMaxHp = enemyTotalHP;
                    float percent = totalHp / totalMaxHp;
                    enemyTotalHPGauge.transform.parent.GetChild(2).GetChild(0).GetComponent<Text>().text = Mathf.RoundToInt(percent * 100f).ToString() + "%";
                    enemyTotalHPGauge.GetComponent<Image>().fillAmount = percent;

                    if (enemyBuilding.hp < enemyBuilding.maxHp) {
                        enemyBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(true); // 건물 하위에 있는 체력게이지 활성화.
                        float hpScaleX = enemyHp / enemyMaxHp;
                        enemyBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(hpScaleX, 1, 1);
                    }
                    if (enemyBuilding.hp <= 0) {
                        float hpScaleX = enemyHp / enemyMaxHp;
                        enemyBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(0, 1, 1);
                        enemyBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                        enemyBuilding.hp = 0;
                        BuildingDestroyed(enemyBuilding);
                    }

                    if (enemyCurrentTotalHP < 0) {
                        enemyTotalHPGauge.GetComponent<Image>().fillAmount = 0;
                        enemyCurrentTotalHP = 0;
                        enemyTotalHPGauge.transform.parent.GetChild(2).GetChild(0).GetComponent<Text>().text = 0.ToString() + "%";
                    }
                }
                break;
            case Target.ME:
                foreach (int tileNum in numbers) {
                    BuildingInfo myBuilding = myBuildingsInfo.Find(x => x.tileNum == tileNum);
                    if (myBuilding == null) return false;
                    myBuilding.hp -= amount;
                    float playerHp = myBuilding.hp;
                    float playerMaxHp = myBuilding.maxHp;
                    if (myBuilding.hp < myBuilding.maxHp) {
                        myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(true); // 건물 하위에 있는 체력게이지 활성화.
                        float hpScaleX = playerHp / playerMaxHp;
                        myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(hpScaleX, 1, 1);
                    }

                    if (myBuilding.hp < 0) {
                        float hpScaleX = playerHp / playerMaxHp;
                        myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(0, 1, 1);
                        myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                        BuildingDestroyed(myBuilding);
                    }
                }
                break;
        }
        return true;
    }

    private void BuildingDestroyed(BuildingInfo buildingInfo) {
        buildingInfo.hp = 0;
        IngameScoreManager.Instance.AddScore(buildingInfo.cardInfo.rarity, IngameScoreManager.ScoreType.DestroyBuilding);
        buildingInfo.activate = false;
        SetWreck(buildingInfo.gameObject);

        if (buildingInfo.gameObject.GetComponent<BuildingObject>().data.card.id == "great_power_stone") {
            GameObject detector = buildingInfo.gameObject.transform.Find("Detector").gameObject;
            if (detector != null) {
                detector.GetComponent<Tower_Detactor>().enabled = false;
            }
        }

        buildingInfo.gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }

    public void SetEnemyBuildingLists(ref GameObject tilegroup) {
        foreach (Transform tile in tilegroup.transform) {
            if (tile.childCount == 1) {
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

    IEnumerator Repair() {
        while (ingameSceneUIController.isPlaying == true) { // playerCity -> MyTerritory -> content -> Haorizontal Scroll Snap -> UICanvas
            yield return new WaitForSeconds(60f);
            for (int i = 0; i < enemyBuildingsInfo.Count; i++) {
                RepairBuilding(Target.ENEMY_1, i);
                RepairDestroyBuilding(Target.ENEMY_1, i);
            }
        }
    }

    public void DecideUnActiveBuilding() {
        while (true) {
            int num = UnityEngine.Random.Range(0, 9);
            if (myBuildingsInfo[num].cardInfo.type == "HQ")
                continue;
            if (myBuildingsInfo[num].activate == false)
                continue;
            else {
                unactiveBuildingIndex = num;
                SetColor(myBuildingsInfo[num].gameObject, Color.red);
                Debug.Log(myBuildingsInfo[num].cardInfo.name + " 비활성화 예정");
                unActiveAlert = true;
                StartCoroutine(StartAlert());
                return;
            }
        }
    }

    IEnumerator StartAlert() {
        int index = unactiveBuildingIndex;
        while (unActiveAlert) {
            if (unActiveAlert)
                SetColor(myBuildingsInfo[index].gameObject, Color.red);
            yield return new WaitForSeconds(0.4f);
            if (unActiveAlert)
                SetColor(myBuildingsInfo[index].gameObject, Color.white);
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void CancleUnActiveBuilding() {
        unActiveAlert = false;
        SetColor(myBuildingsInfo[unactiveBuildingIndex].gameObject, Color.white);
        Debug.Log(myBuildingsInfo[unactiveBuildingIndex].cardInfo.name + " 비활성화 예정 해제");
        unactiveBuildingIndex = 100;
    }

    public void SetUnactiveBuilding() {
        BuildingInfo bi = myBuildingsInfo[unactiveBuildingIndex];
        bi.activate = false;
        switch (bi.cardInfo.prodType) {
            case "gold":
                productResources.gold.food -= bi.cardInfo.product.food;
                productResources.gold.gold -= bi.cardInfo.product.gold;
                productResources.gold.environment -= bi.cardInfo.product.environment;
                break;
            case "food":
                productResources.food.food -= bi.cardInfo.product.food;
                productResources.food.gold -= bi.cardInfo.product.gold;
                productResources.food.environment -= bi.cardInfo.product.environment;
                break;
            case "env":
                productResources.env.food -= bi.cardInfo.product.food;
                productResources.env.gold -= bi.cardInfo.product.gold;
                productResources.env.environment -= bi.cardInfo.product.environment;
                break;
            default:
                BuildingObject buildingObject = bi.gameObject.GetComponent<BuildingObject>();
                string id = buildingObject.data.card.id;
                if (buildingObject.data.card.unit == null || string.IsNullOrEmpty(buildingObject.data.card.unit.name)) {
                    ingameDeckShuffler.DeactiveCard(id, false, bi.gameObject);
                }
                else {
                    ingameDeckShuffler.DeactiveCard(id, true, bi.gameObject);
                }
                break;
        }
        Debug.Log(bi.cardInfo.name + " 비활성화");
        unActiveAlert = false;
        unactiveBuildingIndex = 100;
        StartCoroutine(UnActivateForTime(bi));
    }

    IEnumerator UnActivateForTime(BuildingInfo card) {
        StartCoroutine(UnActivateTimer());
        SetColor(card.gameObject, Color.red);
        yield return new WaitForSeconds(1.0f);
        SetColor(card.gameObject, Color.gray);
        yield return new WaitForSeconds(29.0f);
        card.activate = true;
        SetColor(card.gameObject, Color.white);
        Debug.Log(card.cardInfo.name + " 활성화");
        switch (card.cardInfo.prodType) {
            case "gold":
                productResources.gold.food += card.cardInfo.product.food;
                productResources.gold.gold += card.cardInfo.product.gold;
                productResources.gold.environment += card.cardInfo.product.environment;
                break;
            case "food":
                productResources.food.food += card.cardInfo.product.food;
                productResources.food.gold += card.cardInfo.product.gold;
                productResources.food.environment += card.cardInfo.product.environment;
                break;
            case "env":
                productResources.env.food += card.cardInfo.product.food;
                productResources.env.gold += card.cardInfo.product.gold;
                productResources.env.environment += card.cardInfo.product.environment;
                break;
            default:
                BuildingObject buildingObject = card.gameObject.GetComponent<BuildingObject>();
                string id = buildingObject.data.card.id;
                if (buildingObject.data.card.unit == null || string.IsNullOrEmpty(buildingObject.data.card.unit.name)) {
                    ingameDeckShuffler.ActivateCard(id, false, card.gameObject);
                }
                else {
                    ingameDeckShuffler.ActivateCard(id, true, card.gameObject);
                }
                break;
        }
    }

    IEnumerator UnActivateTimer() {
        GameObject time = Instantiate(unactiveImage, unactiveGroup.transform);
        int leftTime = 30;
        while (leftTime >= 1) {
            yield return new WaitForSeconds(1.0f);
            leftTime--;
            time.transform.GetChild(0).GetComponent<Text>().text = leftTime.ToString();
        }
        Destroy(time);
    }


    /*
    IEnumerator Damage() {
        while (transform.parent.parent.parent.parent.GetComponent<IngameSceneUIController>().isPlaying == true) {
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < enemyBuildingsInfo.Count; i++) {
                TakeDamage(Target.ENEMY_1, i, 20);
            }
        }
    }
    */

    public void SetEnemyTotalHP() {
        for (int i = 0; i < enemyBuildingsInfo.Count; i++) {
            BuildingInfo enemyBuilding = enemyBuildingsInfo.Find(x => x.tileNum == i);
            if (enemyBuilding == null) continue;
            enemyTotalHP += enemyBuilding.hp;
        }
        float totalHp = enemyTotalHP;
        enemyCurrentTotalHP = enemyTotalHP;
        enemyTotalHPGauge.GetComponent<Image>().fillAmount = totalHp / totalHp;
    }

    private void SetColor(GameObject setBuilding, Color color) {
        SpriteRenderer spriteRenderer = setBuilding.GetComponent<SpriteRenderer>();
        if(spriteRenderer != null) {
            spriteRenderer.color = color;
        }
        else {
            setBuilding.GetComponent<SkeletonAnimation>().skeleton.SetColor(color);
        }
    }

    private void SetWreck(GameObject setBuilding) {
        SpriteRenderer spriteRenderer = setBuilding.GetComponent<SpriteRenderer>();
        if(spriteRenderer != null) {
            spriteRenderer.sprite = wreckSprite;
        }
        else {
            SkeletonAnimation ani = setBuilding.GetComponent<SkeletonAnimation>();
            StartCoroutine(SetAnimationTile(ani, wreckSpine));
        }
    }

    private void SetReviveImage(GameObject setBuilding) {
        BuildingObject buildingObject = setBuilding.GetComponent<BuildingObject>();
        SpriteRenderer spriteRenderer = setBuilding.GetComponent<SpriteRenderer>();
        if(spriteRenderer != null) {
            spriteRenderer.sprite = buildingObject.mainSprite;
        }
        else {
            SkeletonAnimation ani = setBuilding.GetComponent<SkeletonAnimation>();
            StartCoroutine(SetAnimationTile(ani, buildingObject.spine));
        }
    }

    private IEnumerator SetAnimationTile(SkeletonAnimation ani, SkeletonDataAsset skeleton) {
        skeleton.GetSkeletonData(false);
        ani.ClearState();
        yield return new WaitForSeconds(0.01f);
        ani.skeletonDataAsset = skeleton;
        ani.Initialize(true);
        ani.AnimationState.SetAnimation(0, skeleton.GetSkeletonData(false).Animations.Items[0], true);
    }


    public enum Target {
        ME,
        ENEMY_1
    }
}
