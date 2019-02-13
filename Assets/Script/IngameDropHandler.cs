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
        if (ingameCityManager.CurrentView == 0) {
            Debug.Log("아군 지역입니다 카드 사용을 취소합니다.");
            return;
        }
        
        if(!IsCardDropOK()) return;

        IngameCard card = selectedObject.GetComponent<IngameCard>();
        object data = card.data;
        if(data.GetType() == typeof(Skill)) SkillActive(data);
        else if(data.GetType() == typeof(Unit)) UnitSummon(data);
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

    private void UnitSummon(object data) {
        Unit unit = (Unit)data;
        if (!CheckResouceOK(unit.cost)) return;

        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction);
        foreach(RaycastHit2D hit in hits)
            if (hit.collider.tag == "EnemyBuilding")
                return;

        //임시 유닛 소환, 유닛 종류 늘면은 그에 대한 대처가 필요함
        var tmp = ingameCityManager.eachPlayersTileGroups;
        GameObject wolf = Instantiate(unitPrefs[0], ((GameObject)tmp[0]).transform);
        wolf.transform.position = ray.origin + new Vector2(0f, 50f);//hit.transform.position;

        UseResource(unit.cost);
        IngameScoreManager.Instance.AddScore(unit.tearNeed, IngameScoreManager.ScoreType.ActiveCard);
        playerController.PrintResource();
        ingameDeckShuffler.UseCard(selectedObject.GetComponent<Index>().Id);
    }

    private void SkillActive(object data) {
        Skill skill = (Skill)data;
        if (!CheckResouceOK(skill.cost)) return;
        if (!canSpell) {
            Debug.Log("스킬 쿨타임!");
            return;
        }

        StartCoroutine(CoolTime());
        ingameCityManager.gameObject.AddComponent<Temple_Damager>().GenerateAttack(skill.method);
        ingameCityManager.gameObject.GetComponent<Temple_Damager>().magma = magma;

        UseResource(skill.cost);
        IngameScoreManager.Instance.AddScore(skill.tierNeed, IngameScoreManager.ScoreType.ActiveCard);
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
