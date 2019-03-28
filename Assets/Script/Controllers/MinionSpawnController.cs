using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using Sirenix.OdinInspector;


public partial class MinionSpawnController : SerializedMonoBehaviour {

    int minionLayer;
    // Use this for initialization
    void Start () {
        if (playerNum == PlayerController.Player.PLAYER_1)
            minionLayer = 10;
        else
            minionLayer = 11;
        StartCoroutine(MinionDelay());
    }
	
	// Update is called once per frame
	void Update () {

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
            if(i < 3)
                minions[i] = Instantiate(shortDisMinion, transform.GetChild(5));
            else
                minions[i] = Instantiate(longDisMinion, transform.GetChild(5));
            minions[i].GetComponent<MinionAI>().SetUnitData(null);
            minions[i].layer = minionLayer;
            minions[i].transform.position = transform.GetChild(i).position;
        }
    }
}

public partial class MinionSpawnController : SerializedMonoBehaviour {
    [Header(" - Player Identity")]
    [SerializeField] PlayerController.Player playerNum;

    [Header(" - Prefabs")]
    [SerializeField] GameObject shortDisMinion;
    [SerializeField] GameObject longDisMinion;


}