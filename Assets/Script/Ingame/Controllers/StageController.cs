using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageController : MonoBehaviour {
	private void Awake() {
		StartCoroutine(SceneLoad());
	}

	IEnumerator SceneLoad() {
		yield return SceneManager.LoadSceneAsync("Stage2", LoadSceneMode.Additive);
		Scene ingame = SceneManager.GetSceneByName("IngameScene");
		Scene stage = SceneManager.GetSceneByName("Stage2");
		SceneManager.MergeScenes(stage, ingame);
	}
}