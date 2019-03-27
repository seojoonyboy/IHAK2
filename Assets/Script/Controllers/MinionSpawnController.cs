using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


public partial class MinionSpawnController : SerializedMonoBehaviour {

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

public partial class MinionSpawnController : SerializedMonoBehaviour {
    [Header(" - Prefabs")]
    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<string, GameObject> minionPrefabs;

    [SerializeField] Transform summonParent;

    //public void HeroSummon(ActiveCard card) {
    //    var result = GetHeroPrefab(card.baseSpec.unit.id);
    //    if (result == null) {
    //        Debug.LogError("해당 유닛의 프리팹을 찾을 수 없습니다!");
    //        return;
    //    }
    //    GameObject hero = Instantiate(result, summonParent);
    //    UnitAI unitAI = hero.GetComponent<UnitAI>();
    //    unitAI.SetUnitData(card);
    //}

    //public GameObject GetHeroPrefab(string id) {
    //    if (!heroPrefabs.ContainsKey(id)) return null;
    //    return heroPrefabs[id];
    //}
}