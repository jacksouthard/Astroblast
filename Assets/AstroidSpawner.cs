using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstroidSpawner : MonoBehaviour {
	public GameObject[] astroids;
	public GameObject alienPrefab;
	public GameObject crystalPrefab;
	public float minSize;
	public float maxSize;
	float size;

	Vector2[] verts;
	List<int> usedIndexes = new List<int> (); // so 2 things dont spawn on the same side

	void Start () {
		GameObject newAstroid = Instantiate (astroids [Random.Range (0, astroids.Length)], transform);

		size = Random.Range (minSize, maxSize);
		newAstroid.transform.localScale = new Vector3 (size, size, 1f);

		verts = newAstroid.GetComponent<PolygonCollider2D> ().points;

		// spawn potential aliens
		int alienCount = 0;
		int random = Random.Range (0, 100);
		if (random < 7) {
			alienCount = 2;
		} else if (random < 30) {
			alienCount = 1;
		}

		for (int i = 0; i < alienCount; i++) {
			EdgePositionData data = GetRandomSpawnPoint ();
			Vector3 spawnPos3d = new Vector3 (data.point.x, data.point.y, 1.1f);
			GameObject newAlien = Instantiate (alienPrefab, spawnPos3d, data.rotation, transform.parent);
		}

		// spawn potential crystals
		int crystalCount = 0;
		random = Random.Range (0, 100);
		if (random < 10) {
			crystalCount = 2;
		} else if (random < 50) {
			crystalCount = 1;
		}

		for (int i = 0; i < crystalCount; i++) {
			EdgePositionData data = GetRandomSpawnPoint ();
			Vector3 spawnPos3d = new Vector3 (data.point.x, data.point.y, 1.1f);
			GameObject newCrystal = Instantiate (crystalPrefab, spawnPos3d, data.rotation, transform.parent);
		}
	}

	public EdgePositionData GetRandomSpawnPoint () {
		int anchorIndex = Random.Range (0, verts.Length);
		while (usedIndexes.Contains (anchorIndex)) {
			anchorIndex = Random.Range (0, verts.Length);
		}
		usedIndexes.Add (anchorIndex);
		int adjacentIndex = GetAdjacentVertIndex (anchorIndex);
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

		return new EdgePositionData (transformedPoint, rot);
	}

	public int GetAdjacentVertIndex (int index) {
		if (index >= verts.Length - 1) {
			return 0;
		} else {
			return index + 1;
		}
	}

	public struct EdgePositionData {
		public Vector2 point;
		public Quaternion rotation;

		public EdgePositionData (Vector2 _point, Quaternion _rotation) {
			point = _point;
			rotation = _rotation;
		}
	}
}
