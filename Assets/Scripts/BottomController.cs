using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomController : MonoBehaviour {
	public static BottomController instance;

	public GameObject crystalPrefab;
	TreasureData data;
	public TreasureData[] allTreasures;
	bool expressed = false;

	void Awake () {
		instance = this;
	}

	void Start () {
		
	}

	public void AttemptExpression () {
		if (!expressed) {
			ExpressSpawns ();
		}
	}

	void ExpressSpawns () {
		Transform spawns = transform.Find ("Spawns");
		List<Transform> cspawns = new List<Transform> ();
		Transform tspawn = null;
		for (int i = 0; i < spawns.childCount; i++) {
			Transform child = spawns.GetChild (i);
			if (child.name.Contains("C")) {
				cspawns.Add (child);
			} else if (child.name.Contains("T")) {
				tspawn = child;
			}
		}

		// spawn crystals
		foreach (var spawn in cspawns) {
			Instantiate (crystalPrefab, spawn.position, spawn.rotation, transform);
		}

		// spawn treasure
		if (tspawn != null) {
			TreasureData tData = allTreasures [TerrainManager.instance.curAstroidIndex];
			Instantiate (tData.prefab, tspawn.position, tspawn.rotation, transform);
		}

		expressed = true;
	}

	[System.Serializable]
	public struct TreasureData {
		public GameObject prefab;
	}
}
