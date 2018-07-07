using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalSpawner : MonoBehaviour {
	[Header("Generation")]
	public GameObject crystalSprout;
	public int maxSprouts;
	public float sproutMaxAngle;
	public float minSproutLength;
	public float maxSproutLength;

	[Header("Collection")]
	public float collectionTime;
	float collectionTimer;
	bool collecting = false;
	Vector3 startPos;
	Transform player; 
	float startSize;

	int valueTeir;

	void Start () {
		int highestValue = TerrainManager.instance.curDifficulty;
		if (highestValue > allCrystalTeirs.Length - 1) {
			highestValue = allCrystalTeirs.Length - 1;
		}

		if (highestValue == 0) {
			valueTeir = highestValue;
		} else {
			valueTeir = Random.Range (highestValue - 1, highestValue + 1);
		}
		// TODO valueTeir += baseValue for astroid

		int sproutCount = Random.Range (1, maxSprouts + 1);
		for (int i = 0; i < sproutCount; i++) {
			Quaternion spawnRot = Quaternion.Euler (0f, 0f, Random.Range (-sproutMaxAngle, sproutMaxAngle) + 90f);
			GameObject newSprout = Instantiate (crystalSprout, transform.position, Quaternion.identity, transform);
			newSprout.transform.localRotation = spawnRot;
			newSprout.GetComponentInChildren<SpriteRenderer> ().color = allCrystalTeirs[valueTeir].color;

			float scale = Random.Range (minSproutLength, maxSproutLength);
			newSprout.transform.localScale = new Vector3 (scale, scale, 1f);
		}

		GetComponent<Light> ().color = allCrystalTeirs[valueTeir].color;
	}

	void Update () {
		if (collecting) {
			collectionTimer -= Time.deltaTime;
			if (collectionTimer <= 0f) {
                // finish collection
                GameController.instance.CollectMoney(allCrystalTeirs[valueTeir].value);
				Destroy (gameObject);
			} else {
				float timeRatio = collectionTimer / collectionTime;
				transform.localScale = Vector3.one * Mathf.Lerp (0.1f, startSize, timeRatio);
				transform.position = Vector3.Lerp (player.position, startPos, timeRatio);
			}
		}
	}

	void StartCollect (Transform target) {
		player = target;
		collecting = true;
		collectionTimer = collectionTime;
		startPos = transform.position;
		startSize = transform.localScale.x; // y should be the same
		GetComponent<Light> ().enabled = false;
	}

	void OnTriggerEnter2D (Collider2D coll) {
		if (!collecting && coll.tag == "Player") {
			StartCollect (coll.transform);
		}
	}

	public CrystalTeir[] allCrystalTeirs;
	[System.Serializable]
	public class CrystalTeir {
		public Color color;
		public int value;
	}
}
