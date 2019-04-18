using UnityEngine;
using System.Collections;

public class circular_animate : MonoBehaviour {
    SpriteRenderer renderer;
	// Use this for initialization
    void Awake() {
        renderer = GetComponent<SpriteRenderer>();
    }
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        renderer.material.SetFloat("_Amount", (Mathf.Sin(Time.time) + 1) * 0.5f);
	}
}
