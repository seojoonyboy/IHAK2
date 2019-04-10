using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using Sirenix.OdinInspector;


public partial class MinionSpawnController : SerializedMonoBehaviour {

    int minionLayer;
    // Use this for initialization
    int healthBuff;
    void Start () {
        if (playerNum == PlayerController.Player.PLAYER_1)
            minionLayer = 10;
        else
            minionLayer = 11;
        //StartCoroutine(MinionDelay());
    }

    IEnumerator MinionDelay() {
        yield return new WaitForSeconds(20.0f);
        SpawnMinion();
        if(PlayerController.Instance.IsPlaying)
            StartCoroutine(MinionDelay());
    }

    private void SpawnMinion() {
        GameObject[] minions = new GameObject[5];
        for(int i = 0; i < 5; i++) {
            if(i < 3) minions[i] = Instantiate(shortDisMinion, transform.GetChild(5));
            else minions[i] = Instantiate(longDisMinion, transform.GetChild(5));

            if(playerNum == PlayerController.Player.PLAYER_1) {
                var effectModules = PlayerController.Instance.PlayerPassiveCards().effectModules;
                if (effectModules.ContainsKey("Unit_health")) {
                    minions[i].GetComponent<MinionAI>().health += effectModules["Unit_health"];
                    Debug.Log("아군 미니언 체력 버프 적용됨");
                }
            }

            minions[i].GetComponent<MinionAI>().SetUnitData(null, null);
            minions[i].layer = minionLayer;
            minions[i].transform.position = transform.GetChild(i).position;
        }
    }

    public void SpawnSquadMinion(string type, int maxSpawn) {
        int spawnNum = 0;
        if (maxSpawn * 10 > (int)PlayerController.Instance.playerResource().Citizen)
            spawnNum = maxSpawn;
        else
            spawnNum = (int)PlayerController.Instance.playerResource().Citizen / 10;
        for (int i = 0; i < spawnNum; i++) {
            if (type == "melee")
                Instantiate(shortDisMinion, SpawnPos.GetChild(i));
            else if(type == "range")
                Instantiate(longDisMinion, SpawnPos.GetChild(i));
        }
    }
}

public partial class MinionSpawnController : SerializedMonoBehaviour {
    [Header(" - Player Identity")]
    [SerializeField] PlayerController.Player playerNum;
    [SerializeField] Transform SpawnPos;

    [Header(" - Prefabs")]
    [SerializeField] GameObject shortDisMinion;
    [SerializeField] GameObject longDisMinion;


}