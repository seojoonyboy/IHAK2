using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        if(!string.IsNullOrEmpty(card.data.skill.name)) SkillActive(card.data.skill);
        else if(!string.IsNullOrEmpty(card.data.unit.name)) UnitSummon(card.data.unit);
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

    private void UnitSummon(Unit data) {
        if (!CheckResouceOK(data.cost)) return;

        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction);
        foreach(RaycastHit2D hit in hits)
            if (hit.collider.tag == "EnemyBuilding")
                return;

        //임시 유닛 소환, 유닛 종류 늘면은 그에 대한 대처가 필요함
        var tmp = ingameCityManager.eachPlayersTileGroups;
        for(int i = 0; i < data.count; i++) {
            float randomPosX = Random.Range(-50f, 50f);
            float randomPosY = Random.Range(-50f, 50f);
            GameObject wolf = Instantiate(unitPrefs[0], ((GameObject)tmp[0]).transform);
            wolf.GetComponent<UnitAI>().SetUnitData(data);
            wolf.transform.position = ray.origin + new Vector2(randomPosX, 50f + randomPosY);//hit.transform.position;
        }

        UseResource(data.cost);
        IngameScoreManager.Instance.AddScore(data.tierNeed, IngameScoreManager.ScoreType.ActiveCard);
        playerController.PrintResource();
        ingameDeckShuffler.UseCard(selectedObject.GetComponent<Index>().Id);
    }

    private void SkillActive(Skill data) {
        if (!CheckResouceOK(data.cost)) return;
        if (!canSpell) {
            Debug.Log("스킬 쿨타임!");
            return;
        }

        StartCoroutine(CoolTime());
        ingameCityManager.gameObject.AddComponent<Temple_Damager>().GenerateAttack(data.method);
        ingameCityManager.gameObject.GetComponent<Temple_Damager>().magma = magma;

        UseResource(data.cost);
        IngameScoreManager.Instance.AddScore(data.tierNeed, IngameScoreManager.ScoreType.ActiveCard);
        playerController.PrintResource();
        ingameDeckShuffler.UseCard(selectedObject.GetComponent<Index>().Id);
    }

    private bool CheckResouceOK(Cost cost) {
        if(playerController.isEnoughResources(cost)) {
            return true;
        }
        else {
            Debug.Log("자원이 부족합니다.");
            return false;
        }
    }

    private void UseResource(Cost cost) {
        playerController.resourceClass.gold -= cost.gold;
        playerController.resourceClass.food -= cost.food;
        playerController.resourceClass.environment -= cost.environment;
    }

    IEnumerator CoolTime() {
        canSpell = false;
        yield return new WaitForSeconds(7.0f);
        canSpell = true;
    }
}
