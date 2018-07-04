using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Starfield : MonoBehaviour {
	public GameObject starPrefab;
	public int starCount;
	public float width;
	public float height;

	void Start () {
		for (int i = 0; i < starCount; i++) {
			Vector3 localPos = new Vector3 (Random.Range (-width, width), Random.Range (-height, height), 10f);
			Instantiate (starPrefab, transform.TransformPoint (localPos), Quaternion.identity, transform);
		}
	}
}
