using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstroidSpawner : MonoBehaviour {
	public GameObject[] astroids;
	public GameObject[] alienPrefabs;
	public GameObject crystalPrefab;
	public float minSize;
	public float maxSize;
	float size;

	public DifficultyData[] allDifficultyData;
	DifficultyData diffData;

	Transform persistantObjects; // container for aliens and astroids which dont despawn when they leave the screen on the way down

	Vector2[] verts;
	List<int> usedIndexes = new List<int> (); // so 2 things dont spawn on the same side

	void Start () {
		// set difficulty 
		int difficulty = TerrainManager.instance.curDifficulty;
		if (difficulty > allDifficultyData.Length - 1) {
			difficulty = allDifficultyData.Length - 1;
		}
		diffData = allDifficultyData [Mathf.Clamp(difficulty, 0, allDifficultyData.Length - 1)];
		
		persistantObjects = GameObject.Find ("PersistantObjects").transform;
		transform.parent = persistantObjects;
		
		// spawn astroid
		GameObject newAstroid = Instantiate (astroids [Random.Range (0, astroids.Length)], transform);
		transform.rotation = Quaternion.Euler (0f, 0f, Random.Range (0f, 360f));
		size = Random.Range (minSize, maxSize);
		newAstroid.transform.localScale = new Vector3 (size, size, 1f);
		verts = newAstroid.GetComponent<PolygonCollider2D> ().points;

		if (alienPrefabs.Length != 0) {
			// spawn potential aliens
			int alienCount = Random.Range (diffData.minAliens, diffData.maxAliens + 1);

			for (int i = 0; i < alienCount; i++) {
				EdgePositionData data = GetRandomSpawnPoint ();
				Vector3 spawnPos3d = new Vector3 (data.point.x, data.point.y, 1.1f);
				// chose prefab from avaiable aliens in difficulty teir
				GameObject prefab = alienPrefabs [diffData.alienIDs[Random.Range(0, diffData.alienIDs.Length)]];
				GameObject newAlien = Instantiate (prefab, spawnPos3d, data.rotation, persistantObjects);

				newAlien.GetComponent<BasicAlien> ().InitPos (this, data.anchorIndex);
			}
		}

		// spawn potential crystals
		int crystalCount = Random.Range (diffData.minCrystals, diffData.maxCrystals + 1);

		for (int i = 0; i < crystalCount; i++) {
			EdgePositionData data = GetRandomSpawnPoint ();
			Vector3 spawnPos3d = new Vector3 (data.point.x, data.point.y, 1.1f);
			Instantiate (crystalPrefab, spawnPos3d, data.rotation, persistantObjects);
		}
	}

	public EdgePositionData GetRandomSpawnPoint () {
		int anchorIndex = Random.Range (0, verts.Length);
		while (usedIndexes.Contains (anchorIndex)) {
			anchorIndex = Random.Range (0, verts.Length);
		}
		usedIndexes.Add (anchorIndex);
		int adjacentIndex = GetAdjacentVertIndex (anchorIndex, 1);
		return GetPosBetweenVerts (anchorIndex, adjacentIndex, 0.5f);
	}

	public EdgePositionData GetPosBetweenVerts (int startIndex, int endIndex, float ratio) {
		// calculate position
		Vector2 startPos = verts [startIndex];
		Vector2 endPos = verts [endIndex];
		Vector2 pos = Vector2.Lerp (startPos, endPos, ratio);
		Vector2 transformedPoint = transform.TransformPoint (pos * size);
//		print ("T: " + transform.position + "B: " + pos + " A: " + transformedPoint);

		// calculate rotation
		Vector2 dir = (startPos - endPos).normalized;
		var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
		Quaternion adjustedRot = Quaternion.Euler (rot.eulerAngles.x, rot.eulerAngles.y, rot.eulerAngles.z + transform.eulerAngles.z);

		return new EdgePositionData (startIndex, transformedPoint, adjustedRot);
	}

    public float GetDistBetweenVerts(int startIndex, int endIndex) {
        return Vector2.Distance(verts[startIndex], verts[endIndex]);
    }

	public int GetAdjacentVertIndex (int index, int dir) {
        return (index + verts.Length + dir) % verts.Length;
	}

	public struct EdgePositionData {
        public int anchorIndex;
		public Vector2 point;
		public Quaternion rotation;

		public EdgePositionData (int _anchorIndex, Vector2 _point, Quaternion _rotation) {
            anchorIndex = _anchorIndex;
            point = _point;
			rotation = _rotation;
		}
	}

	[System.Serializable]
	public class DifficultyData {
		public int[] alienIDs;
		public int minAliens;
		public int maxAliens;
		public int minCrystals;
		public int maxCrystals;
	}
}
