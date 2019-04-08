using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using UnityEngine.UI;
using Spine.Unity;
using Container;
using System;

public class IngameHpSystem : Singleton<IngameHpSystem> {
    protected IngameHpSystem() { }

    [Header (" - Player")]
    public GameObject playerController;
    public MyBuildings myBuildings;
    public PlayerResource myResource;
    public List<BuildingInfo> myBuildingInfo;
    public GameObject playerhpGauge;
    public BuildingInfo playerHQ;
    

    [Space(10)]
    [Header(" - Enemy")]
    public GameObject enemyController;
    public EnemyBuildings enemyBuildings;
    public PlayerResource enemyResource;    
    public List<BuildingInfo> enemybuildingInfos;
    public GameObject enemyhpGauge;
    public BuildingInfo enemyHQ;

    [Space(10)]
    [Header(" - Wreck")]
    [SerializeField] private Sprite wreckSprite;
    [SerializeField] private SkeletonDataAsset wreckSpine;

    IngameSceneEventHandler ingameSceneEventHandler;
    public IngameSceneUIController ingameSceneUIController;

    void Awake() {
        ingameSceneEventHandler = IngameSceneEventHandler.Instance;
        ingameSceneEventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.ENEMY_BUILDINGS_INFO_ADDED, SetEnemy);
        ingameSceneEventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, SetPlayer);
    }

    private void SetEnemy(Enum Event_Type, Component Sender, object Param) {
        enemyBuildings = enemyController.GetComponent<EnemyBuildings>();
        enemyResource = enemyController.GetComponent<PlayerResource>();
        enemybuildingInfos = enemyBuildings.buildingInfos;
        enemyHQ = enemybuildingInfos.Find(x => x.tileNum == 12);
        
        SetHp();
        //TakeDamage(Target.ME, 12, 1500);
    }
    private void SetPlayer(Enum Event_Type, Component Sender, object Param) {
        myBuildings = playerController.GetComponent<MyBuildings>();
        myBuildingInfo = myBuildings.buildingInfos;
        myResource = playerController.GetComponent<PlayerResource>();
        playerHQ = myBuildingInfo.Find(x => x.tileNum == 12);
    }

    void OnDestroy() {
        ingameSceneEventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.ENEMY_BUILDINGS_INFO_ADDED, SetEnemy);
        ingameSceneEventHandler.RemoveListener(IngameSceneEventHandler.EVENT_TYPE.MY_BUILDINGS_INFO_ADDED, SetPlayer);
    }


    public void SetHp() {
        enemyhpGauge.transform.Find("hpHeader").Find("hpValue").GetComponent<Text>().text = 100.ToString() + "%";
        enemyhpGauge.transform.Find("HpBar").GetComponent<Image>().fillAmount = 100f;

        playerhpGauge.transform.Find("hpHeader").Find("hpValue").GetComponent<Text>().text = 100.ToString() + "%";
        playerhpGauge.transform.Find("HpBar").GetComponent<Image>().fillAmount = 100f;
    }

    IEnumerator attack() {
        while(enemyHQ.hp > 0) {
            yield return new WaitForSeconds(1.0f);
            TakeDamage(Target.ENEMY_1, 100);
        }
    }

    /*
    void Awake() {
        ingameSceneEventHandler = IngameSceneEventHandler.Instance;
        ingameSceneEventHandler.AddListener(IngameSceneEventHandler.EVENT_TYPE.TAKE_DAMAGE, TakeDamageEventOcccured);
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
    }*/




    public bool TakeDamage(Target target, int amount) {
        switch (target) {
            case Target.ENEMY_1:
                if (enemyHQ == null) return false;
                if (enemyHQ.hp <= 0) return false;

                //체력감소 연산
                if (enemyHQ.hp > amount) {
                    enemyHQ.hp -= amount;
                    if (enemyHQ.hp < enemyHQ.maxHp) {
                        enemyHQ.gameObject.transform.GetChild(0).gameObject.SetActive(true); // 건물 하위에 있는 체력게이지 활성화.
                        float hpScaleX = (float)enemyHQ.hp / enemyHQ.maxHp;
                        enemyHQ.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(hpScaleX, 1, 1);

                        enemyhpGauge.transform.Find("hpHeader").Find("hpValue").GetComponent<Text>().text = Mathf.RoundToInt(hpScaleX * 100f).ToString() + "%";
                        enemyhpGauge.transform.Find("HpBar").GetComponent<Image>().fillAmount = hpScaleX;
                    }
                }
                else if (enemyHQ.hp <= amount) {
                    amount = enemyHQ.hp;
                    enemyHQ.hp = 0;
                    enemyHQ.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(0, 1, 1);
                    enemyHQ.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    enemyHQ.activate = false;
                    BuildingDestroyed(target, enemyHQ);
                    enemyhpGauge.transform.Find("hpHeader").Find("hpValue").GetComponent<Text>().text = 0f.ToString() + "%";
                    enemyhpGauge.transform.Find("HpBar").GetComponent<Image>().fillAmount = 0;

                    if (enemyHQ.hp <= 0)
                        DestroyEnemy();
                }

                /*
                if (enemyResource.TotalHp > amount) {
                    enemyResource.TotalHp -= amount;
                    float percent = (float)enemyResource.TotalHp / enemyResource.maxhp;                    
                }
                else if(enemyResource.TotalHp <= amount) {
                    enemyResource.TotalHp = 0;                   
                } 
                */
                IngameScoreManager.Instance.AddScore(amount, IngameScoreManager.ScoreType.Attack);
                break;

            case Target.ME:
                if (playerHQ == null) return false;
                if (playerHQ.hp <= 0) return false;


                if(playerHQ.hp > amount) {
                    playerHQ.hp -= amount;

                    if (playerHQ.hp < playerHQ.maxHp) {
                        playerHQ.gameObject.transform.GetChild(0).gameObject.SetActive(true); // 건물 하위에 있는 체력게이지 활성화.
                        float hpScaleX = (float)playerHQ.hp / playerHQ.maxHp;
                        playerHQ.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(hpScaleX, 1, 1);
                        playerhpGauge.transform.Find("hpHeader").Find("hpValue").GetComponent<Text>().text = Mathf.RoundToInt(hpScaleX * 100f).ToString() + "%";
                        playerhpGauge.transform.Find("HpBar").GetComponent<Image>().fillAmount = hpScaleX;
                    }
                }
                else if(playerHQ.hp <= amount) {
                    amount = playerHQ.hp;
                    playerHQ.hp = 0;
                    playerHQ.gameObject.transform.GetChild(0).GetChild(1).localScale = new Vector3(0, 1, 1);
                    playerHQ.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    playerHQ.activate = false;
                    BuildingDestroyed(target, playerHQ);
                    playerhpGauge.transform.Find("hpHeader").Find("hpValue").GetComponent<Text>().text = 0.ToString() + "%";
                    playerhpGauge.transform.Find("HpBar").GetComponent<Image>().fillAmount = 0;

                    if (playerHQ.gameObject.GetComponent<BuildingObject>().setTileLocation == 12)
                        DestroyCity();
                }
                /*
                if(myResource.TotalHp > amount) {
                    myResource.TotalHp -= amount;
                    float percent = (float)myResource.TotalHp / myResource.maxhp;
                    
                }
                else if (myResource.hp <= amount) {
                    myResource.TotalHp = 0;
                                                
                }    
                */
                break;
                
        }
        return true;
    }

    private void SetWreck(GameObject setBuilding) {
        
        SpriteRenderer spriteRenderer = setBuilding.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            spriteRenderer.sprite = wreckSprite;
        }
        else {
            SkeletonAnimation ani = setBuilding.GetComponent<SkeletonAnimation>();
            StartCoroutine(SetAnimationTile(ani, wreckSpine));
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

    private void BuildingDestroyed(Target target, BuildingInfo buildingInfo) {
        buildingInfo.hp = 0;
        IngameScoreManager.Instance.AddScore(buildingInfo.cardInfo.rarity, IngameScoreManager.ScoreType.DestroyBuilding);
        buildingInfo.activate = false;
        SetWreck(buildingInfo.gameObject);

        if (buildingInfo.gameObject.GetComponent<BuildingObject>().card.data.id == "great_power_stone") {
            GameObject detector = buildingInfo.gameObject.transform.Find("Detector").gameObject;
            if (detector != null) {
                detector.GetComponent<Tower_Detactor>().enabled = false;
                buildingInfo.gameObject.transform.GetChild(2).gameObject.SetActive(false);
            }
        }
        IngameSceneEventHandler.BuildingDestroyedPackage package = new IngameSceneEventHandler.BuildingDestroyedPackage() {
            target = target,
            buildingInfo = buildingInfo
        };

        IngameSceneEventHandler.Instance.PostNotification(
            IngameSceneEventHandler.EVENT_TYPE.BUILDING_DESTROYED,
            this,
            package
        );
        /*
        if (target == Target.ME)
            ReduceProductPower(buildingInfo);
            */
        buildingInfo.gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }

    public void DestroyEnemy() {        
        if (enemyResource.hp == 0 && enemyHQ.activate == false) {
            enemyResource.hp = 0;
            enemyhpGauge.transform.Find("HpBar").GetComponent<Image>().fillAmount = 0f;
            enemyhpGauge.transform.Find("hpHeader").Find("hpValue").GetComponent<Text>().text = 0.ToString() + " % ";
            //StopCoroutine("Repair");
        }        
    }

    public void DestroyCity() {        
        if (myResource.hp == 0 || playerHQ.activate == false) {
            myResource.hp = 0;
            playerhpGauge.transform.Find("HpBar").GetComponent<Image>().fillAmount = 0f;
            playerhpGauge.transform.Find("hpHeader").Find("hpValue").GetComponent<Text>().text = 0.ToString() + " % ";
            //StopCoroutine("Repair");
        }         
    }
    
    /*
   public bool TakeDamage(Target target, List<int> numbers, int amount) {
       for (int i = 0; i < numbers.Count; i++) {
           TakeDamage(target, numbers[i], amount);
       }
       return true;
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
               repairAmount += amount;

               //if(playerController.Gold <= (float)(amount / 10)) {
               //    enoughRepairSource = false;
               //    return false;                    
               //}
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
               repairCount++;
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

   public bool RepairDestroyBuilding(Target target, int tileNum) {
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


               if (enemyBuilding.gameObject.GetComponent<BuildingObject>().card.data.id == "great_power_stone") {
                   GameObject detector = enemyBuilding.gameObject.transform.Find("Detector").gameObject;
                   if (detector != null) {
                       detector.GetComponent<Tower_Detactor>().enabled = true;
                       if (detector.GetComponent<Tower_Detactor>().towerShellCount < detector.GetComponent<Tower_Detactor>().towerMaxShell)
                           enemyBuilding.gameObject.transform.GetChild(2).gameObject.SetActive(true);
                   }
               }
               BuildingObject buildingObject = enemyBuilding.gameObject.GetComponent<BuildingObject>();
               string id = buildingObject.card.data.id;

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


   

   


       public void DamagePlayerCity(int damage) {
           for (int i = 0; i < demoTileIndex.Length; i++) {
               TakeDamage(Target.ME, demoTileIndex[i], damage);
           }
       }

   }


   public void RepairPlayerCity() {
       for (int i = 0; i < demoTileIndex.Length; i++) {
           RepairBuilding(Target.ME, demoTileIndex[i]);

           if (enoughRepairSource == false) {
               playerController.activeRepair = false;
               break;
           }

       }

       if (repairCount != 0) {
           uint consume = (uint)(Mathf.RoundToInt(repairAmount / repairCount) / 10);
           Debug.Log(consume);
           //playerController.Gold = playerController.CheckResourceFlow(playerController.Gold, consume, false); 
       }
       else if (repairCount == 0) {
           playerController.activeRepair = false;
       }
       repairAmount = 0;
       repairCount = 0;
       enoughRepairSource = true;
   }
   */
   /*
   IEnumerator attacking() {
        while(ingameSceneUIController.isPlaying == true) {
            yield return new WaitForSeconds(1.0f);
            TakeDamage(Target.ME, 12, 500);
        }
    }
    */

    public enum Target {
        ME,
        ENEMY_1,
        ENEMY_2           
    }
}
