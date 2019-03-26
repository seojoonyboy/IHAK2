using System;
using System.Collections;
using System.Collections.Generic;
using DataModules;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Spine.Unity;
using TMPro;
using Container;

public class IngameCityManager : MonoBehaviour {

    public ArrayList eachPlayersTileGroups = new ArrayList();

    public List<UpgradeInfo> upgradeInfos = new List<UpgradeInfo>();

    [Header(" - TotalHPUI")]
    [SerializeField] private Image hpValueBar;
    [SerializeField] private Text hpValue;
    [SerializeField] private GameObject enemyTotalHPGauge;
    [SerializeField] IngameSceneUIController ingameSceneUIController;


    [Space(10)]

    [Header(" - ProductResource")]
    [SerializeField] public ProductResources productResources;
    [SerializeField] public int goldGenerate;
    [SerializeField] public int envGenerate;
    [SerializeField] public int foodGenerate;

    [Space(10)]

    [Header(" - DeckInfo")]
    [SerializeField] private Deck deck;
    public List<BuildingInfo> enemyBuildingsInfo = new List<BuildingInfo>();
    public Dictionary<string, int> myBuildingsInfo_Keys = new Dictionary<string, int>();
    public List<Magnification> myBuildings_mags = new List<Magnification>();

    [Space(10)]
    [Header(" - PlayerDeck")]
    private int[] demoTileIndex = { 6, 7, 8, 11, 12, 13, 16, 17, 18 };
    public int[] buildingList;
    [SerializeField] private Transform playerCity;


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
    [Header(" - RepairStatus")]
    [SerializeField] public int repairCount = 0;
    [SerializeField] public float repairAmount = 0;
    [SerializeField] public bool enoughRepairSource = true;

    [Space(10)]
    [Header(" - Other")]
    [SerializeField] private Sprite wreckSprite;
    [SerializeField] private SkeletonDataAsset wreckSpine;
    [SerializeField] PlayerController playerController;
    
    
    IngameDeckShuffler ingameDeckShuffler;

    private IEnumerator firstAlert;
    private IEnumerator secondAlert;
    List<BuildingInfo> myBuildingsInfo;

    

    

    // Use this for initialization
    void Start() {
        //deck = AccountManager.Instance.decks[0];
        //enemyTotalHP = 100;
        //enemyCurrentTotalHP = enemyTotalHP;
        //buildingList = deck.coordsSerial;
        //ingameSceneUIController = FindObjectOfType<IngameSceneUIController>();
        //wreckSpine.GetSkeletonData(false);
        //ingameDeckShuffler = GetComponent<IngameDeckShuffler>();

        //cityMaxHP = cityHP;

        //maxHp.text = hpValue.text = cityMaxHP.ToString();
        //hpValue.text = ((int)(cityHP / cityMaxHP) * 100).ToString() + "%";
        //hpValueBar.fillAmount = cityHP / cityMaxHP;
        //InitProduction();

        //productResources = playerCity.GetChild(0).GetComponent<TileGroup>().touchPerProdPower;
        //goldGenerate = productResources.all.gold;
        //foodGenerate = productResources.all.food;
        //envGenerate = productResources.all.environment;

        //SetHQ();
        //SetEnemyTotalHP();
        //StartCoroutine("Repair");
        //StartCoroutine("TakingDamage");
        //TakeDamage();
        //StartCoroutine("Repaircity");

        myBuildingsInfo = playerController.playerBuildings().buildingInfos;
    }

    public Magnification SearchMags(string key) {
        Magnification result = null;
        myBuildings_mags.ForEach(x => { if(x.key.CompareTo(key)==0) result = x; });
        return result;
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
            //cityHP -= 100;
            //hpValue.text = cityHP.ToString();
            //hpValueBar.fillAmount = cityHP / cityMaxHP;
        }
    }


    public void SetEnemyBuildingLists(ref GameObject tilegroup) {
        foreach (Transform tile in tilegroup.transform) {
            if (tile.childCount == 1) {
                int tileNum = tile.GetComponent<TileObject>().tileNum;
                GameObject building = tile.GetChild(0).gameObject;
                BuildingObject buildingObject = building.GetComponent<BuildingObject>();
                CardData card = buildingObject.card.data;

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
    /*
    IEnumerator Repair() {
        while (ingameSceneUIController.isPlaying == true) { // playerCity -> MyTerritory -> content -> Haorizontal Scroll Snap -> UICanvas
            yield return new WaitForSeconds(60f);
            for (int i = 0; i < enemyBuildingsInfo.Count; i++) {
                RepairBuilding(Target.ENEMY_1, i);
                RepairDestroyBuilding(Target.ENEMY_1, i);
            }
        }
    }
    */
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
        //switch (bi.cardInfo.prodType) {
        //    case "gold":
        //        productResources.gold.food -= bi.cardInfo.product.food;
        //        productResources.gold.gold -= bi.cardInfo.product.gold;
        //        productResources.gold.environment -= bi.cardInfo.product.environment;
        //        break;
        //    case "food":
        //        productResources.food.food -= bi.cardInfo.product.food;
        //        productResources.food.gold -= bi.cardInfo.product.gold;
        //        productResources.food.environment -= bi.cardInfo.product.environment;
        //        break;
        //    case "env":
        //        productResources.env.food -= bi.cardInfo.product.food;
        //        productResources.env.gold -= bi.cardInfo.product.gold;
        //        productResources.env.environment -= bi.cardInfo.product.environment;
        //        break;
        //    default:
        //        break;
        //}
        //BuildingObject buildingObject = bi.gameObject.GetComponent<BuildingObject>();
        //string id = buildingObject.card.data.id;
        //if (buildingObject.card.data.activeSkills.Length != 0) {
        //    ingameDeckShuffler.DeactiveCard(buildingObject.gameObject);
        //}
    }

    private void RecoverProductPower(BuildingInfo bi) {
        //switch (bi.cardInfo.prodType) {
        //    case "gold":
        //        productResources.gold.food += bi.cardInfo.product.food;
        //        productResources.gold.gold += bi.cardInfo.product.gold;
        //        productResources.gold.environment += bi.cardInfo.product.environment;
        //        break;
        //    case "food":
        //        productResources.food.food += bi.cardInfo.product.food;
        //        productResources.food.gold += bi.cardInfo.product.gold;
        //        productResources.food.environment += bi.cardInfo.product.environment;
        //        break;
        //    case "env":
        //        productResources.env.food += bi.cardInfo.product.food;
        //        productResources.env.gold += bi.cardInfo.product.gold;
        //        productResources.env.environment += bi.cardInfo.product.environment;
        //        break;
        //    default:
        //        break;
        //}

        //BuildingObject buildingObject = bi.gameObject.GetComponent<BuildingObject>();
        //string id = buildingObject.card.data.id;
        //if (buildingObject.card.data.activeSkills.Length != 0) {
        //    ingameDeckShuffler.ActivateCard(buildingObject.gameObject);
        //}
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
        enemyHQ = enemyBuildingsInfo.Find(x => x.tileNum == 12);
        playerHQ = myBuildingsInfo.Find(x => x.tileNum == 12);
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

    /*
    IEnumerator TakingDamage() {
        while (ingameSceneUIController.isPlaying == true) {
            yield return new WaitForSeconds(1f);
            TakeDamage(Target.ME, 11, 10);
            TakeDamage(Target.ME, 13, 10);
            TakeDamage(Target.ME, 12, 10 );
            
            TakeDamage(Target.ME, 6, 10);
            TakeDamage(Target.ME, 7, 10);
            TakeDamage(Target.ME, 8, 10);

            TakeDamage(Target.ME, 18, 10);
            TakeDamage(Target.ME, 17, 10);
            TakeDamage(Target.ME, 16, 10);

        }
    }

    public void TakeDamage() {
        TakeDamage(Target.ME, 11, 50);
        TakeDamage(Target.ME, 13, 50);
        TakeDamage(Target.ME, 12, 1000);

        TakeDamage(Target.ME, 6, 50);
        TakeDamage(Target.ME, 7, 50);
        TakeDamage(Target.ME, 8, 50);

        TakeDamage(Target.ME, 18, 50);
        TakeDamage(Target.ME, 17, 50);
        TakeDamage(Target.ME, 16, 50);
    }
    */
    /*
    IEnumerator Repaircity() {
        while (ingameSceneUIController.isPlaying == true) {
            yield return new WaitForSeconds(2f);
            RepairBuilding(Target.ME, 6);
            RepairBuilding(Target.ME, 13);
        }
    }*/
    public void ResetProductPower() {
        productResources.all.food = foodGenerate;
        productResources.all.environment = envGenerate;
        productResources.all.gold = goldGenerate;
    }


    
}
