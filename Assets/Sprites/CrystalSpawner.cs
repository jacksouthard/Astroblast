using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalSpawner : MonoBehaviour {
	public Color color;
	public GameObject crystalSprout;
	public int maxSprouts;
	public float sproutMaxAngle;
	public float minSproutLength;
	public float maxSproutLength;

	void Start () {
		int sproutCount = Random.Range (1, maxSprouts + 1);
		for (int i = 0; i < sproutCount; i++) {
			Quaternion spawnRot = Quaternion.Euler (0f, 0f, Random.Range (-sproutMaxAngle, sproutMaxAngle) + 90f);
			GameObject newSprout = Instantiate (crystalSprout, transform.position, Quaternion.identity, transform);
			newSprout.transform.localRotation = spawnRot;
			newSprout.GetComponentInChildren<SpriteRenderer> ().color = color;

			float scale = Random.Range (minSproutLength, maxSproutLength);
			newSprout.transform.localScale = new Vector3 (scale, scale, 1f);
		}

		GetComponent<Light> ().color = color;
	}
}
