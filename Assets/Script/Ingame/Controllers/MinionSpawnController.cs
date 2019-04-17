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

    public void SpawnMinionSquad(ActiveCard heroCard, Transform spawnPos) {
        int spawnNum = 0;
        if (heroCard.baseSpec.unit.minion.count * 10 > (int)PlayerController.Instance.playerResource().Citizen)
            spawnNum = (int)PlayerController.Instance.playerResource().Citizen / 10;
        else
            spawnNum = heroCard.baseSpec.unit.minion.count;
        
        
        for (int i = 0; i < spawnNum; i++) {
            GameObject minion;
            if (heroCard.baseSpec.unit.minion.type == "melee") {
                minion = Instantiate(shortDisMinion, spawnPos);
            }
            else{
                minion = Instantiate(longDisMinion, spawnPos);
            }
            minion.transform.position
                = new Vector3(spawnPos.position.x + Random.Range(-15.0f, 15.0f), spawnPos.position.y + Random.Range(-15.0f, 15.0f), spawnPos.position.z);
            minion.GetComponent<MinionAI>().SetMinionData(heroCard);
            PlayerController.Instance.CitizenSpawnController().DeleteCitizen();
        }
    }
}

public partial class MinionSpawnController : SerializedMonoBehaviour {
    [Header(" - Player Identity")]
    [SerializeField] PlayerController.Player playerNum;

    [Header(" - Prefabs")]
    [SerializeField] GameObject shortDisMinion;
    [SerializeField] GameObject longDisMinion;

    public GameObject ShortDisMinion {
        get { return shortDisMinion; }
    }
    public GameObject LongDisMinion {
        get { return longDisMinion; }
    }

}