using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModules;

public class TileGroup : MonoBehaviour {
    public ProductResources touchPerProdPower;

    //보유중인 유닛카드 정보
    public List<Unit> units;
    //보유중인 액티브 스킬정보 (예 : 마그마)
    public List<Skill> activeSkills;
}
