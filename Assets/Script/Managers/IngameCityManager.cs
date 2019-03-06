using System;
using System.Collections;
using System.Collections.Generic;
using DataModules;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Spine.Unity;
using TMPro;

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

    public List<UpgradeInfo> upgradeInfos = new List<UpgradeInfo>();

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
    [SerializeField] public int cityMaxHP = 0;

    [Space(10)]

    [Header(" - ProductResource")]
    public ProductResources productResources;
    public ProductResources unActiveResources;

    [Space(10)]

    [Header(" - DeckInfo")]
    [SerializeField] private Deck deck;
    public List<BuildingInfo> myBuildingsInfo = new List<BuildingInfo>();
    public List<BuildingInfo> enemyBuildingsInfo = new List<BuildingInfo>();
    public Dictionary<string, int> myBuildingsInfo_Keys = new Dictionary<string, int>();
    public List<Magnification> myBuildings_mags = new List<Magnification>();

    [Space(10)]
    [Header(" - PlayerDeck")]
    private int[] demoTileIndex = { 6, 7, 8, 11, 12, 13, 16, 17, 18 };
    public int[] buildingList;


    [Space(10)]
    [Header(" - UnActive")]
    public GameObject unactiveImage;
    public int unactiveBuildingIndex1 = 100;
    public int unactiveBuildingIndex2 = 100;
    private bool unActiveAlert1 = false;
    private bool unActiveAlert2 = false;

    [Space(10)]
    [Header(" - HQBuildingObject")]
    public BuildingInfo playerHQ;
    public BuildingInfo enemyHQ;


    [Space(10)]
    [Header(" - Other")]
    [SerializeField] private Sprite wreckSprite;
    [SerializeField] private SkeletonDataAsset wreckSpine;
    [SerializeField] PlayerController playerController;
    IngameSceneEventHandler ingameSceneEventHandler;
    IngameDeckShuffler ingameDeckShuffler;

    private IEnumerator firstAlert;
    private IEnumerator secondAlert;

    void Awake() {
        ingameSceneEventHandler = IngameSceneEventHandler.Instance;
        ingameSceneEventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.TAKE_DAMAGE, TakeDamageEventOcccured);
    }

    void OnDestroy() {
        ingameSceneEventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.TAKE_DAMAGE, TakeDamageEventOcccured);
    }

    // Use this for initialization
    void Start() {
        deck = AccountManager.Instance.decks[0];
        buildingList = deck.coordsSerial;
        ingameSceneUIController = FindObjectOfType<IngameSceneUIController>();
        wreckSpine.GetSkeletonData(false);
        ingameDeckShuffler = GetComponent<IngameDeckShuffler>();

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

        var queryGroups =
            from bulding in myBuildingsInfo
            group bulding by bulding.cardInfo.type into newGroup
            orderby newGroup.Key
            select newGroup;

        //myBuildingsInfo.Clear();
        List<BuildingInfo> tmp = new List<BuildingInfo>();
        int count = 0;
        foreach (var group in queryGroups) {
            List<BuildingInfo> list = group.ToList();
            list = list.OrderBy(x => x.cardInfo.prodType).ToList();
            string prev_sub_key = null;
            if(group.Key != "prod") myBuildingsInfo_Keys.Add(group.Key, count);
            foreach (BuildingInfo info in list) {
                if(group.Key == "prod" && prev_sub_key != info.cardInfo.prodType) {
                    myBuildingsInfo_Keys.Add(group.Key + "-" + info.cardInfo.prodType, count);
                    prev_sub_key = info.cardInfo.prodType;
                }
                tmp.Add(info);
                count++;
            }
        }
        myBuildingsInfo = tmp;

        foreach(var key in myBuildingsInfo_Keys) {
            if(key.Key == "military") {
                myBuildings_mags.Add(new Magnification(key.Key, 0.075f, 1.75f, 10));
            }
            else {
                myBuildings_mags.Add(new Magnification(key.Key, 0.1f, 2.0f, 10));
            }
        }

        IEnumerable <GameObject> gameObjects =
            from x in myBuildingsInfo
            select x.gameObject;

        ingameSceneEventHandler.PostNotification(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, this, gameObjects.ToList());

        cityMaxHP = cityHP;

        //maxHp.text = hpValue.text = cityMaxHP.ToString();
        hpValue.text = ((int)(cityHP / cityMaxHP) * 100).ToString() + "%";
        hpValueBar.fillAmount = cityHP / cityMaxHP;
        //InitProduction();

        productResources = transform.GetChild(1).GetComponent<TileGroup>().touchPerProdPower;

        
        SetHQ();
        SetEnemyTotalHP();
        StartCoroutine("Repair");
        //StartCoroutine("TakingDamage");
        //StartCoroutine("Repaircity");
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
                if (enemyBuilding.hp < 0) BuildingDestroyed(target, enemyBuilding);
                break;

            case Target.ME:
                BuildingInfo myBuilding = myBuildingsInfo.Find(x => x.tileNum == tileNum);
                if (myBuilding == null) return false;                
                if (myBuilding.hp >= myBuilding.maxHp) return false;
                if (myBuilding.activate == false) {
                    SetReviveImage(myBuilding.gameObject);
                    myBuilding.activate = true;
                    myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                    RecoverProductPower(myBuilding);
                }


                float playerMaxHp = myBuilding.maxHp;
                int amount = Mathf.RoundToInt(playerMaxHp * 0.04f);
                myBuilding.hp += amount;
                cityHP += amount;

                int overHP;
                if (myBuilding.hp > myBuilding.maxHp) {
                    overHP = myBuilding.hp - myBuilding.maxHp;
                    cityHP -= overHP;
                }

                float playerHp = myBuilding.hp;
                float playerHpScaleX = playerHp / playerMaxHp;
                myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(playerHpScaleX, 1, 1);


                float cityTotalHP = cityMaxHP;
                float cityCurrentHP = cityHP;
                float cityHPpercent = cityCurrentHP / cityTotalHP;
                hpValue.text = Mathf.RoundToInt(cityHPpercent * 100f).ToString() + "%";
                hpValueBar.GetComponent<Image>().fillAmount = cityHPpercent;

                if (myBuilding.hp >= myBuilding.maxHp) {
                    myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(false); // 건물 하위에 있는 체력게이지 활성화.
                    myBuilding.hp = myBuilding.maxHp;
                    myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(1, 1, 1);
                }

                if (cityCurrentHP > cityTotalHP) {
                    cityCurrentHP = cityTotalHP;
                    hpValue.text = 100.ToString() + "%";
                    hpValueBar.fillAmount = 1f;
                }

                if (myBuilding.hp < 0) BuildingDestroyed(target, myBuilding);
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

                if (enemyCurrentTotalHP >= enemyTotalHP) {
                    enemyCurrentTotalHP = enemyTotalHP;
                    enemyTotalHPGauge.transform.parent.GetChild(2).GetChild(0).GetComponent<Text>().text = 100.ToString() + "%";
                    enemyTotalHPGauge.GetComponent<Image>().fillAmount = 1f;
                }

                if (enemyBuilding.hp < 0) BuildingDestroyed(target, enemyBuilding);
                break;

            case Target.ME:
                BuildingInfo myBuilding = myBuildingsInfo.Find(x => x.tileNum == tileNum);
                if (myBuilding == null) return false;
                if (myBuilding.hp >= myBuilding.maxHp) return false;
                if (myBuilding.activate == false) {
                    SetReviveImage(myBuilding.gameObject);
                    myBuilding.activate = true;
                    myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                    RecoverProductPower(myBuilding);
                }

                float playerMaxHp = myBuilding.maxHp;
                myBuilding.hp += amount;
                cityHP += amount;

                int overHP;
                if(myBuilding.hp > myBuilding.maxHp) {
                    overHP = myBuilding.hp - myBuilding.maxHp;
                    cityHP -= overHP;
                }                
                
                float playerHp = myBuilding.hp;
                float playerHpScaleX = playerHp / playerMaxHp;
                myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(playerHpScaleX, 1, 1);


                float cityTotalHP = cityMaxHP;
                float cityCurrentHP = cityHP;
                float cityHPpercent = cityCurrentHP / cityTotalHP;
                hpValue.text = Mathf.RoundToInt(cityHPpercent * 100f).ToString() + "%";
                hpValueBar.GetComponent<Image>().fillAmount = cityHPpercent;

                if (myBuilding.hp > myBuilding.maxHp) {
                    myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(false); // 건물 하위에 있는 체력게이지 활성화.
                    myBuilding.hp = myBuilding.maxHp;
                    myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(1, 1, 1);
                }

                if (cityCurrentHP > cityTotalHP) {
                    cityCurrentHP = cityTotalHP;
                    hpValue.text = 100.ToString() + "%";
                    hpValueBar.fillAmount = 1f;
                }

                if (myBuilding.hp < 0) BuildingDestroyed(target, myBuilding);
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
                    if (myBuilding.hp > 0) {
                        myBuilding.hp = myBuilding.maxHp;
                        RecoverProductPower(myBuilding);
                    }
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
                        if(detector.GetComponent<Tower_Detactor>().towerShellCount < detector.GetComponent<Tower_Detactor>().towerMaxShell)
                            enemyBuilding.gameObject.transform.GetChild(2).gameObject.SetActive(true);
                    }
                }
                BuildingObject buildingObject = enemyBuilding.gameObject.GetComponent<BuildingObject>();
                string id = buildingObject.data.card.id;
                if (buildingObject.data.card.unit != null || buildingObject.data.card.activeSkills.Length != 0) {
                    ingameDeckShuffler.ActivateCard(id, enemyBuilding.gameObject);
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
                    BuildingDestroyed(target, enemyBuilding);

                    if (enemyBuilding.gameObject.GetComponent<BuildingObject>().data.id == -1)
                        DestroyEnemy();
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
                if (myBuilding.hp <= 0) return false;

                myBuilding.hp -= amount;
                cityHP -= amount;
                float playerHp = myBuilding.hp;
                float playerMaxHp = myBuilding.maxHp;
                

                int overDamage;
                if(myBuilding.hp < 0) {
                    overDamage = 0 - myBuilding.hp;
                    cityHP += overDamage;
                }

                float cityTotalHP = cityMaxHP;
                float cityCurrentHP = cityHP;
                float cityHPpercent = cityCurrentHP / cityTotalHP;
                hpValue.text = Mathf.RoundToInt(cityHPpercent * 100f).ToString() + "%";
                hpValueBar.GetComponent<Image>().fillAmount = cityHPpercent;

                if (myBuilding.hp < myBuilding.maxHp) {
                    myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(true); // 건물 하위에 있는 체력게이지 활성화.
                    float hpScaleX = playerHp / playerMaxHp;
                    myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(hpScaleX, 1, 1);
                }

                if (myBuilding.hp <= 0) {
                    float hpScaleX = playerHp / playerMaxHp;
                    myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(0, 1, 1);
                    myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    myBuilding.hp = 0;
                    BuildingDestroyed(target, myBuilding);
                    
                    if (myBuilding.gameObject.GetComponent<BuildingObject>().data.id == -1)
                        DestroyCity();
                        
                }

                if (cityHP < 0) {
                    float hpScaleX = playerHp / playerMaxHp;
                    myBuilding.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(0, 1, 1);
                    myBuilding.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    BuildingDestroyed(target, myBuilding);
                }
                break;
        }
        return true;
    }

    public bool TakeDamage(Target target, List<int> numbers, int amount) {
        for(int i=0; i<numbers.Count; i++) {
            TakeDamage(target, numbers[i], amount);
        }
        return true;
    }

    private void BuildingDestroyed(Target target, BuildingInfo buildingInfo) {
        buildingInfo.hp = 0;
        IngameScoreManager.Instance.AddScore(buildingInfo.cardInfo.rarity, IngameScoreManager.ScoreType.DestroyBuilding);
        buildingInfo.activate = false;
        SetWreck(buildingInfo.gameObject);

        if (buildingInfo.gameObject.GetComponent<BuildingObject>().data.card.id == "great_power_stone") {
            GameObject detector = buildingInfo.gameObject.transform.Find("Detector").gameObject;
            if (detector != null) {
                detector.GetComponent<Tower_Detactor>().enabled = false;
                buildingInfo.gameObject.transform.GetChild(2).gameObject.SetActive(false);
            }
        }

        if(target == Target.ME)
            ReduceProductPower(buildingInfo);
        /*
        if(buildingInfo.cardInfo.id == -1) {

        }
        */
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
            if (unactiveBuildingIndex1 == num)
                continue;
            else {
                if (unactiveBuildingIndex1 == 100) {
                    unactiveBuildingIndex1 = num;
                    unActiveAlert1 = true;
                    firstAlert = StartAlert1();
                    StartCoroutine(firstAlert);
                }
                else {
                    unactiveBuildingIndex2 = num;
                    unActiveAlert2 = true;
                    secondAlert = StartAlert2();
                    StartCoroutine(secondAlert);
                }
                //SetColor(myBuildingsInfo[num].gameObject, Color.red);
                Debug.Log(myBuildingsInfo[num].cardInfo.name + " 비활성화 예정");
                
                return;
            }
        }
    }

    IEnumerator StartAlert1() {
        int index = unactiveBuildingIndex1;
        while (unActiveAlert1) {
            if (unActiveAlert1)
                SetColor(myBuildingsInfo[index].gameObject, Color.red);
            yield return new WaitForSeconds(0.4f);
            if (unActiveAlert1)
                SetColor(myBuildingsInfo[index].gameObject, Color.white);
            yield return new WaitForSeconds(0.2f);
        }
    }
    IEnumerator StartAlert2() {
        int index = unactiveBuildingIndex2;
        while (unActiveAlert2) {
            if (unActiveAlert2)
                SetColor(myBuildingsInfo[index].gameObject, Color.red);
            yield return new WaitForSeconds(0.4f);
            if (unActiveAlert2)
                SetColor(myBuildingsInfo[index].gameObject, Color.white);
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void CancleUnActiveBuilding() {
        
        if (unactiveBuildingIndex2 == 100) {
            unActiveAlert1 = false;
            StopCoroutine(firstAlert);
            SetColor(myBuildingsInfo[unactiveBuildingIndex1].gameObject, Color.white);
            Debug.Log(myBuildingsInfo[unactiveBuildingIndex1].cardInfo.name + " 비활성화 예정 해제");
            unactiveBuildingIndex1 = 100;
        }
        else {
            unActiveAlert2 = false;
            StopCoroutine(secondAlert);
            SetColor(myBuildingsInfo[unactiveBuildingIndex2].gameObject, Color.white);
            Debug.Log(myBuildingsInfo[unactiveBuildingIndex2].cardInfo.name + " 비활성화 예정 해제");
            unactiveBuildingIndex2 = 100;
        }
        
    }

    public void SetUnactiveBuilding() {
        BuildingInfo bi = new BuildingInfo();
        for (int i = 0; i < 2; i++) {
            if (i == 0)
                bi = myBuildingsInfo[unactiveBuildingIndex1];
            else
                bi = myBuildingsInfo[unactiveBuildingIndex2];
            bi.activate = false;
            ReduceProductPower(bi);
            StartCoroutine(UnActivateForTime(bi));
        }
        unActiveAlert1 = unActiveAlert2 = false;
        unactiveBuildingIndex1 = unactiveBuildingIndex2 = 100;
    }

    private void ReduceProductPower(BuildingInfo bi) {
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
                break;
        }
        BuildingObject buildingObject = bi.gameObject.GetComponent<BuildingObject>();
        string id = buildingObject.data.card.id;
        if (buildingObject.data.card.unit != null || buildingObject.data.card.activeSkills.Length != 0) {
            ingameDeckShuffler.DeactiveCard(id, buildingObject.gameObject);
        }
    }

    private void RecoverProductPower(BuildingInfo bi) {
        switch (bi.cardInfo.prodType) {
            case "gold":
                productResources.gold.food += bi.cardInfo.product.food;
                productResources.gold.gold += bi.cardInfo.product.gold;
                productResources.gold.environment += bi.cardInfo.product.environment;
                break;
            case "food":
                productResources.food.food += bi.cardInfo.product.food;
                productResources.food.gold += bi.cardInfo.product.gold;
                productResources.food.environment += bi.cardInfo.product.environment;
                break;
            case "env":
                productResources.env.food += bi.cardInfo.product.food;
                productResources.env.gold += bi.cardInfo.product.gold;
                productResources.env.environment += bi.cardInfo.product.environment;
                break;
            default:
                break;
        }
        BuildingObject buildingObject = bi.gameObject.GetComponent<BuildingObject>();
        string id = buildingObject.data.card.id;
        if (buildingObject.data.card.unit != null || buildingObject.data.card.activeSkills.Length != 0) {
            ingameDeckShuffler.ActivateCard(id, buildingObject.gameObject);
        }
    }


    IEnumerator UnActivateForTime(BuildingInfo card) {
        StartCoroutine(UnActivateTimer(card.gameObject));
        SetColor(card.gameObject, Color.red);
        yield return new WaitForSeconds(1.0f);
        SetColor(card.gameObject, Color.black);
        yield return new WaitForSeconds(29.0f);
        card.activate = true;
        SetColor(card.gameObject, Color.white);
        Debug.Log(card.cardInfo.name + " 활성화");

        RecoverProductPower(card);
    }

    IEnumerator UnActivateTimer(GameObject building) {
        GameObject time = Instantiate(unactiveImage, transform);
        time.transform.position = building.transform.position;
        int leftTime = 30;
        while (leftTime >= 1) {
            yield return new WaitForSeconds(1.0f);
            leftTime--;
            time.transform.GetChild(1).GetComponent<TextMeshPro>().text = leftTime.ToString();
        }
        Destroy(time);
    }

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

    public void SetHQ() {
        enemyHQ = enemyBuildingsInfo.Find(x => x.tileNum == enemyBuildingsInfo.Count / 2);
        playerHQ = myBuildingsInfo.Find(x => x.tileNum == 12);
    }

    public void DestroyEnemy() {
        if(enemyHQ.hp == 0 && enemyHQ.activate == false) {
            enemyCurrentTotalHP = 0;
            enemyTotalHPGauge.GetComponent<Image>().fillAmount = 0f;
            enemyTotalHPGauge.transform.parent.GetChild(2).GetChild(0).GetComponent<Text>().text = 0.ToString() + " % ";
            StopCoroutine("Repair");
        }
    }

    public void DestroyCity() {
        if(playerHQ.hp == 0 && playerHQ.activate == false) {
            cityHP = 0;
            hpValueBar.fillAmount = 0f;
            hpValue.text = 0.ToString() + " % ";
            StopCoroutine("Repair");
        }

    }

    public void RepairPlayerCity() {
        for(int i = 0; i < demoTileIndex.Length; i++) {
            RepairBuilding(Target.ME, demoTileIndex[i]);
        }
    }

    public void DamagePlayerCity(int damage) {
        for (int i = 0; i < demoTileIndex.Length; i++) {
            TakeDamage(Target.ME, demoTileIndex[i], damage);
        }
    }

    public int CityDestroyBuildingCount() {
        int count = 0;
        BuildingInfo myBuilding;
        for (int i = 0; i < demoTileIndex.Length; i++) {
            myBuilding = myBuildingsInfo.Find(x => x.tileNum == demoTileIndex[i]);
            if (myBuilding.activate == false)
                count++;
        }
        return count;
    }

    public int CityTotalTileCount() {
        int count = 0;
        for(int i = 0; i< demoTileIndex.Length; i++) 
            count++;      

        return count;
    }


    IEnumerator TakingDamage() {
        while (ingameSceneUIController.isPlaying == true) {
            yield return new WaitForSeconds(1f);
            TakeDamage(Target.ME, 6, 10 );
            TakeDamage(Target.ME, 7, 10);
            TakeDamage(Target.ME, 8, 10);
            TakeDamage(Target.ME, 13, 5);
        }
    }

    IEnumerator Repaircity() {
        while (ingameSceneUIController.isPlaying == true) {
            yield return new WaitForSeconds(2f);
            RepairBuilding(Target.ME, 6);
            RepairBuilding(Target.ME, 13);
        }
    }


    public enum Target {
        ME,
        ENEMY_1
    }
}
