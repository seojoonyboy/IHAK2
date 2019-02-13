using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using TMPro;
using DataModules;

public class TileGroup : MonoBehaviour {
    public ProductResources touchPerProdPower;
    [Header (" - buildingInformation")]
    public List<int> attackingTowerLocation;
    public int tileCount;

    [Header (" - Active or Unit")]
    //보유중인 유닛카드 정보
    public List<DataModules.Unit> units;
    //보유중인 액티브 스킬정보 (예 : 마그마)
    public List<Skill> activeSkills;

    [Header(" - Flag")]
    public bool isGame = false;

    [Header (" - GameSystemData")]
    public IngameSceneUIController ingameSceneUIController;


    private void Start() {
        tileCount = transform.childCount - 1;        
        SettingBuildingForGame();
        CheckingAttackBuilding();
        SetAmmo();
        StartCoroutine("ReloadTowerAmmo");
    }

    public void CheckingAttackBuilding() {
        if (isGame == false) return;
        GameObject tile;
        GameObject building;
        for (int i = 0; i<tileCount; i++) {
            tile = transform.GetChild(i).gameObject;
            building = (tile.transform.childCount > 0) ? tile.transform.GetChild(0).gameObject : null;

            if (building == null)
                continue;

            BuildingObject buildingObject = building.GetComponent<BuildingObject>();
            if (building != null && buildingObject.data.card.type == "military" &&  buildingObject.data.card.attackInfo.id > 0)
                attackingTowerLocation.Add(tile.GetComponent<TileObject>().tileNum);
        }
    }

    public void SettingBuildingForGame() {
        ingameSceneUIController = FindObjectOfType<IngameSceneUIController>();
        if (ingameSceneUIController == null) return;
        isGame = ingameSceneUIController.isPlaying;
        ingameSceneUIController.ObserveEveryValueChanged(_ => ingameSceneUIController.isPlaying).Subscribe(_ => isGame = (ingameSceneUIController.isPlaying == true) ? true : false);
    }

    public void SetAmmo() {
        if (isGame == false)
            return;
        GameObject building;
        Tower_Detactor detector;
        TextMeshPro ammoValueText;

        for (int i = 0; i < attackingTowerLocation.Count; i++) {
            building = transform.GetChild(attackingTowerLocation[i]).GetChild(0).gameObject;
            detector = building.transform.GetChild(1).GetComponent<Tower_Detactor>();
            ammoValueText = building.transform.GetChild(2).GetComponent<TextMeshPro>();

            detector.towerShellCount += detector.towerMaxShell;
            ammoValueText.text = detector.towerShellCount + " / " + detector.towerMaxShell;
            ammoValueText.transform.gameObject.SetActive(false);
            if (detector.towerShellCount > detector.towerMaxShell)
                detector.towerShellCount = detector.towerMaxShell;
        }

    }

    IEnumerator ReloadTowerAmmo() {
        if (isGame == false) 
            yield return null;
        

        while(isGame == true) {
            yield return new WaitForSeconds(40.0f);
            GameObject building;
            Tower_Detactor detector;
            TextMeshPro ammoValueText;

            for (int i = 0; i<attackingTowerLocation.Count; i++) {
                building = transform.GetChild(attackingTowerLocation[i]).GetChild(0).gameObject;
                detector = building.transform.GetChild(1).GetComponent<Tower_Detactor>();
                ammoValueText = building.transform.GetChild(2).GetComponent<TextMeshPro>();

                detector.towerShellCount += detector.towerMaxShell / 2;
                if (detector.towerShellCount > detector.towerMaxShell) {
                    detector.towerShellCount = detector.towerMaxShell;
                    ammoValueText.transform.gameObject.SetActive(false);
                }
                ammoValueText.text = detector.towerShellCount + " / " + detector.towerMaxShell;
            }
        }
    }

}
