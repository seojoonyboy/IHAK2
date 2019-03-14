using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class IngameDropHandler : MonoBehaviour {
    public GameObject selectedObject;
    public GameObject[] unitPrefs;

    [SerializeField] IngameCityManager ingameCityManager;
    [SerializeField] PlayerController playerController;
    [SerializeField] IngameDeckShuffler ingameDeckShuffler;
    [SerializeField] Sprite magma;

    Camera cam;
    bool canSpell = true;

    // Use this for initialization
    void Start() {
        cam = GameObject.Find("TerritoryCamera").GetComponent<Camera>();
    }

    public void OnDrop() {
        //if (ingameCityManager.CurrentView == 0) {
        //    Debug.Log("아군 지역입니다 카드 사용을 취소합니다.");
        //    return;
        //}
        
        if(!IsCardDropOK()) return;

        ActiveCardInfo card = selectedObject.GetComponent<ActiveCardInfo>();
        int rarity = card.data.parentBuilding.GetComponent<BuildingObject>().data.card.rarity;
        if(!string.IsNullOrEmpty(card.data.baseSpec.skill.name) && ingameCityManager.CurrentView != 0) SkillActive(card.data.baseSpec.skill, rarity);
        else if(!string.IsNullOrEmpty(card.data.baseSpec.unit.name)) UnitSummon(card.data, rarity);
        
    }

    private bool IsCardDropOK() {
        GraphicRaycaster m_Raycaster = GetComponentInParent<GraphicRaycaster>();
        PointerEventData m_PointEventData = new PointerEventData(FindObjectOfType<EventSystem>());
        m_PointEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointEventData, results);

        const string StrB = "Horizontal Scroll Snap";
        return results[0].gameObject.name.CompareTo(StrB) == 0;
    }

    private void UnitSummon(ActiveCard card, int rarity) {
        if (ingameCityManager.CurrentView == 0) return;
        if (!CheckResouceOK(card.baseSpec.unit.cost)) return;
        
        Unit data = card.baseSpec.unit;
        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction);
        foreach(RaycastHit2D hit in hits)
            if (hit.collider.tag == "EnemyBuilding")
                return;

        //임시 유닛 소환, 유닛 종류 늘면은 그에 대한 대처가 필요함
        var tmp = ingameCityManager.eachPlayersTileGroups;
        int summonPos = ingameCityManager.CurrentView == 0 ? 1 : 0; //아군지역에 소환인지 적군지역에 소환인지
        //int layer = summonPos == 1 ? LayerMask.NameToLayer("PlayerUnit") : LayerMask.NameToLayer("EnemyUnit");
        //tmp[0] EnemyCity의 TileGroup_DummyEnemy
        //tmp[1] PlayerCity의 TileGroup_Empty_x로 이동 됨
        GameObject wolf = Instantiate(unitPrefs[0], ((GameObject)tmp[summonPos]).transform);
        //wolf.layer = layer;
        UnitAI unitAI = wolf.GetComponent<UnitAI>();
        unitAI.SetUnitData(card);
        if(data.id != "n_uu_0101") {
            GameObject Name = wolf.transform.Find("Name").gameObject;
            Name.SetActive(true);
            Name.GetComponent<TextMeshPro>().text = data.name;
        }
        unitAI.protecting = summonPos == 1;
        wolf.transform.position = ray.origin + new Vector2(0f, 50f);//hit.transform.position;

        IngameCityManager.BuildingInfo buildingInfos = ingameCityManager.myBuildingsInfo.Find(x=>x.tileNum == card.parentBuilding.GetComponent<BuildingObject>().setTileLocation);
        buildingInfos.activate = false;
        buildingInfos.gameObject.GetComponent<TileSpineAnimation>().SetUnit(false);

        UseResource(data.cost);
        IngameScoreManager.Instance.AddScore(data.tierNeed, IngameScoreManager.ScoreType.ActiveCard, 0, rarity);
        playerController.PrintResource();
        ingameDeckShuffler.UseCard(selectedObject);

        Debug.Log(selectedObject.transform.GetSiblingIndex());
    }

    private void SkillActive(Skill data, int rarity) {
        if (!CheckResouceOK(data.cost)) return;
        if (!canSpell) {
            IngameAlarm.instance.SetAlarm("스킬 쿨타임입니다!");
            Debug.Log("스킬 쿨타임!");
            return;
        }

        StartCoroutine(CoolTime());

        if(data.method.methodName == "skill_magma") {
            ingameCityManager.gameObject.AddComponent<Temple_Damager>().GenerateAttack(data.method, IngameCityManager.Target.ENEMY_1);
            ingameCityManager.gameObject.GetComponent<Temple_Damager>().magma = magma;
        }

        UseResource(data.cost);
        IngameScoreManager.Instance.AddScore(data.tierNeed, IngameScoreManager.ScoreType.ActiveCard, 0, rarity);
        playerController.PrintResource();
        ingameDeckShuffler.UseCard(selectedObject);
    }

    private bool CheckResouceOK(Cost cost) {
        if(playerController.isEnoughResources(cost)) {
            return true;
        }
        else {
            Debug.Log("자원이 부족합니다.");
            IngameAlarm.instance.SetAlarm("자원이 부족합니다!");
            return false;
        }
    }

    private void UseResource(Cost cost) {
        playerController.resourceClass.gold -= (uint)cost.gold;
        playerController.resourceClass.food -= (uint)cost.food;
        playerController.resourceClass.environment -= cost.environment;
    }

    IEnumerator CoolTime() {
        canSpell = false;
        yield return new WaitForSeconds(7.0f);
        canSpell = true;
    }
}
