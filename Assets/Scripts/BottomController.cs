using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomController : MonoBehaviour {
	public static BottomController instance;

	public GameObject crystalPrefab;
	TreasureData data;
	public TreasureData[] allTreasures;
	bool expressed = false;
	bool alreadyGotTreasure = false;
	public bool hasTreasure = false;
	public bool alwaysSpawnTreasure;

	void Awake () {
		instance = this;
	}

	void Start () {
		int boolInt = PlayerPrefs.GetInt ("T" + TerrainManager.instance.curAstroidIndex, 0); // 0 means false | 1 is true
		if (boolInt == 1 && !alwaysSpawnTreasure) {
			alreadyGotTreasure = true;
			print ("Already have treasure");
		}
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
		if (tspawn != null && !alreadyGotTreasure && allTreasures.Length - 1 >= TerrainManager.instance.curAstroidIndex) {
			TreasureData tData = allTreasures [TerrainManager.instance.curAstroidIndex];
			Instantiate (tData.prefab, tspawn.position, tspawn.rotation, transform);
		}

		expressed = true;
	}

	public void TreasureCollected () {
		hasTreasure = true;

		MessageManager.instance.OnCollectTreasure ();
	}

	public void TreasureExtracted () {
		GameController.instance.UnlockNewAstroid (TerrainManager.instance.curAstroidIndex + 1);
		GameController.instance.CollectMoney (allTreasures [TerrainManager.instance.curAstroidIndex].value);
		PlayerPrefs.SetInt ("T" + TerrainManager.instance.curAstroidIndex, 1);
	}

	[System.Serializable]
	public struct TreasureData {
		public int value;
		public GameObject prefab;
	}
}
