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
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnDrop() {
        GraphicRaycaster m_Raycaster = GetComponentInParent<GraphicRaycaster>();
        PointerEventData m_PointEventData = new PointerEventData(FindObjectOfType<EventSystem>());
        m_PointEventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointEventData, results);
        if(results[0].gameObject.name.CompareTo("Horizontal Scroll Snap")!=0) return;
        Vector3 origin = cam.ScreenToWorldPoint(Input.mousePosition);
        Ray2D ray = new Ray2D(origin, Vector2.zero);
        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction);
        foreach(RaycastHit2D hit in hits) {
            Debug.Log(hit.collider.tag);
            Debug.Log(hit.collider.name);
            IngameCard card = selectedObject.GetComponent<IngameCard>();
            object data = card.data;

            if (hit.collider.tag == "BackGroundTile") {
                if (ingameCityManager.CurrentView == 0) {
                    Debug.Log("아군 도시에는 유닛생산 불가");
                    return;
                }
                //Debug.Log(hit.collider.name);
                var tmp = ingameCityManager.eachPlayersTileGroups;
                
                if(data.GetType() == typeof(Unit)) {
                    Unit unit = (Unit)data;
                    if (playerController.isEnoughResources(unit.cost)) {
                        GameObject wolf = Instantiate(unitPrefs[0], ((GameObject)tmp[0]).transform);
                        wolf.transform.position = hit.point + new Vector2(0f, 50f);//hit.transform.position;

                        playerController.resourceClass.gold -= unit.cost.gold;
                        playerController.resourceClass.food -= unit.cost.food;
                        playerController.resourceClass.environment -= unit.cost.environment;

                        IngameScoreManager.Instance.AddScore(unit.tearNeed, IngameScoreManager.ScoreType.ActiveCard);

                        playerController.PrintResource();
                        ingameDeckShuffler.UseCard(selectedObject.GetComponent<Index>().Id);
                    }
                    else {
                        Debug.Log("자원 부족");
                    }
                }
            }

            if(data.GetType() == typeof(Skill)) {
                if (ingameCityManager.CurrentView == 0) {
                    Debug.Log("아군 도시에는 스펠 불가");
                    return;
                }
                Debug.Log(canSpell);
                if (canSpell) {
                    //Debug.Log("주문 공격");
                    StartCoroutine(CoolTime());

                    Skill skill = (Skill)data;
                    if (playerController.isEnoughResources(skill.cost)) {
                        ingameCityManager.gameObject.AddComponent<Temple_Damager>().GenerateAttack(skill.method);
                        ingameCityManager.gameObject.GetComponent<Temple_Damager>().magma = magma;
                        IngameScoreManager.Instance.AddScore(skill.tierNeed, IngameScoreManager.ScoreType.ActiveCard);

                        playerController.resourceClass.gold -= skill.cost.gold;
                        playerController.resourceClass.food -= skill.cost.food;
                        playerController.resourceClass.environment -= skill.cost.environment;

                        playerController.PrintResource();

                        ingameDeckShuffler.UseCard(selectedObject.GetComponent<Index>().Id);
                    }
                    else {
                        Debug.Log("자원 부족");
                    }
                }
                else {
                    Debug.Log("스킬 쿨타임!");
                }
            }
        }
    }

    IEnumerator CoolTime() {
        canSpell = false;
        yield return new WaitForSeconds(7.0f);
        canSpell = true;
    }
}
