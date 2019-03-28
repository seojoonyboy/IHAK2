using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using Sirenix.OdinInspector;


public partial class MinionSpawnController : SerializedMonoBehaviour {

    // Use this for initialization
    void Start () {
        StartCoroutine(MinionDelay());
        GameObject hero = Instantiate(shortDisMinion, transform);
    }
	
	// Update is called once per frame
	void Update () {

	}

    IEnumerator MinionDelay() {
        yield return new WaitForSeconds(20.0f);
        StartCoroutine(SpawnMinion());
    }

    IEnumerator SpawnMinion() {
        yield return new WaitForSeconds(1.0f);
    }
}

public partial class MinionSpawnController : SerializedMonoBehaviour {
    [Header(" - Player Identity")]
    [SerializeField] PlayerController.Player playerNum;

    [Header(" - Prefabs")]
    [SerializeField] GameObject shortDisMinion;
    [SerializeField] GameObject longDisMinion;


}