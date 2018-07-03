﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TerrainManager : MonoBehaviour {
	public bool spawnTerrain;

	[Header("Chunks")]
	public float objectSpawnBuffer;
	public float objectDespawnDst;
	List<LevelChunk> allLevelChunks = new List<LevelChunk>();
	[HideInInspector]
	public List<GameObject> activeObjects = new List<GameObject>();

	[Header("Teirs")]
	public float dstPerTeir;
	public int chunksPerTeir;
	int teir = 0;
	int maxTeir;

//	[Header("Stars")]
//	public GameObject starPrefab;
//	public float starParalaxScale;
//	public float starSpawnIntervals;
//	public float initialStarCount;
//	public float initialStarX;
//	Transform lastStar = null;

	[Header("Background Objects")]
	public GameObject[] backgroundPrefabs;
	public float backgroundParalaxScale;
	public float backgroundSpawnIntervals;
	public float initialBackgroundCount;
	public float initialBackgroundY;
	Transform lastBackground = null;

	[Header("Side Pieces")]
	public GameObject[] sidePiecePrefabs;
	public float sidePieceX;
	public float sidePieceSpawnIntervals;
	public float initialSidePieceCount;
	public float initialSidePieceY;
	Transform lastSidePiece = null;

	[HideInInspector]
	public float farthestY = 0f;
	Transform player;
	float curChunkY = 0f;

	void Awake () {
		BuildLevelChunks ();
	}

	void Start () {
		player = GameObject.Find ("Player").transform;

		maxTeir = Mathf.FloorToInt (allLevelChunks.Count / chunksPerTeir);

		// spawn initial chunks
		ResetTerrain();
	}
		
	void Update () {
		if (player.transform.position.y > farthestY) {
			farthestY = player.transform.position.y;

			if (teir < maxTeir) {
				if (farthestY > (teir + 1) * dstPerTeir) {
					teir++;
					print ("Increasing difficutly to teir " + teir);
				}
			}

			if (spawnTerrain) {
				if (farthestY + objectSpawnBuffer > curChunkY) {
					ExpressNextChunk ();
				}
			}

//			if (lastStar == null) {
//				CreateNextStar ();
			//			} else if (farthestY + objectSpawnBuffer > lastStar.position.x) {
//				CreateNextStar ();
//			}
//
			if (lastBackground == null) {
				CreateNextBackground ();
			} else if (farthestY + objectSpawnBuffer > lastBackground.position.y) {
				CreateNextBackground ();
			}

			if (lastSidePiece == null) {
				CreateNextSidePiece ();
			} else if (farthestY + objectSpawnBuffer > lastSidePiece.position.y) {
				CreateNextSidePiece ();
			}

			TestForObjectDespawns ();
		}
	}

	public void ResetTerrain () {
		ClearObjects ();

//		initialStarX += player.position.y;
		initialBackgroundY += player.position.y;
		initialSidePieceY += player.position.y;

		farthestY = 0f;
		curChunkY = 0f;

		if (spawnTerrain) {
			for (int i = 0; i < 3; i++) {
				ExpressNextChunk ();
			}
		}

//		for (int i = 0; i < initialStarCount; i++) {
//			CreateNextStar ();
//		}
//
		for (int i = 0; i < initialBackgroundCount; i++) {
			CreateNextBackground ();
		}

		for (int i = 0; i < initialSidePieceCount; i++) {
			CreateNextSidePiece ();
		}
	}

//	void CreateNextStar () {
//		Vector3 spawnPos;
//		if (lastStar == null) {
//			spawnPos = new Vector3 (initialStarX, Random.Range (-50, 30), 50f);
//		} else {
//			spawnPos = new Vector3 (lastStar.position.x + starSpawnIntervals, Random.Range (-50, 30), 50f);
//		}
//		GameObject star = (GameObject)Instantiate (starPrefab, spawnPos, Quaternion.identity, transform);
//		star.GetComponent<Paralax> ().Init (spawnPos, starParalaxScale);
//		lastStar = star.transform;
//
//		activeObjects.Add (star);
//	}
//
	void CreateNextBackground () {
		
		Vector3 spawnPos;
		if (lastBackground == null) {
			spawnPos = new Vector3 (0f, initialBackgroundY, 20f);
		} else {
			spawnPos = new Vector3 (0f, lastBackground.position.y + backgroundSpawnIntervals, 20f);
		}
		GameObject backgroundPrefab = backgroundPrefabs[Random.Range (0, backgroundPrefabs.Length)];
		GameObject background = (GameObject)Instantiate (backgroundPrefab, spawnPos, Quaternion.identity, transform);
		background.GetComponent<Paralax> ().Init (spawnPos, backgroundParalaxScale);
		lastBackground = background.transform;

		if (Random.value > 0.5f) {
			// flip background
			background.transform.localScale = new Vector3 (-background.transform.localScale.x, background.transform.localScale.y, 1f);
		}

		activeObjects.Add (background);
	} 

	void CreateNextSidePiece () {
		Vector3 spawnPos;
		if (lastSidePiece == null) {
			spawnPos = new Vector3 (sidePieceX, initialSidePieceY, 0f);
		} else {
			spawnPos = new Vector3 (sidePieceX, lastSidePiece.position.y + sidePieceSpawnIntervals, 0f);
		}
		GameObject prefab = sidePiecePrefabs [Random.Range (0, sidePiecePrefabs.Length)];

		for (int sign = -1; sign <= 1; sign += 2) {
			Vector3 newSpawnPos = new Vector3 (sidePieceX * sign, spawnPos.y, spawnPos.z);
			GameObject piece = (GameObject)Instantiate (prefab, newSpawnPos, Quaternion.identity, transform);
			lastSidePiece = piece.transform;

			piece.transform.localScale = new Vector3 (piece.transform.localScale.x * -sign, piece.transform.localScale.y, 1f);

			activeObjects.Add (piece);
		}
	}

	void ExpressNextChunk () {
		LevelChunk chunk = GetChunk ();
		int xFlipSign = 1;
		if (Random.value > 0.5f) {
			xFlipSign = -1;
		}
	
		foreach (var levelObject in chunk.levelObjects) {
			Vector3 spawnPos = new Vector3 (levelObject.pos.x * xFlipSign, levelObject.pos.y + curChunkY, 1f);
			Quaternion spawnRot = Quaternion.Euler (0f, 0f, Random.Range (0f, 360f));
			GameObject GO = (GameObject)Instantiate (levelObject.prefab, spawnPos, spawnRot, transform);
			activeObjects.Add (GO);
		}

		curChunkY += chunk.chunkSize;
	}

	LevelChunk GetChunk () {
		int maxIndex = Mathf.Clamp ((teir * chunksPerTeir) + 1, 0, allLevelChunks.Count - 1);
		int random = Random.Range (0, maxIndex + 1);

//		print ("Max Index: " + maxIndex + " Random: " + random);


		return allLevelChunks [random];
	}

	void TestForObjectDespawns () {
		float despawnMargin = farthestY - objectDespawnDst;
		List<GameObject> objectsToDelete = new List<GameObject> ();
		for (int i = 0; i < activeObjects.Count; i++) {
			if (activeObjects [i] != null) {
				if (activeObjects [i].transform.position.y < despawnMargin) {
					objectsToDelete.Add (activeObjects [i].gameObject);
				}
			}
		}
		foreach (var GO in objectsToDelete) {
			activeObjects.Remove (GO);
			Destroy (GO);

		}
			
	}

	void SetSeed (int _seed) {
		Random.InitState (_seed);
	}

	void ClearObjects () {
		for (int i = 0; i < activeObjects.Count; i++) {
			Destroy (activeObjects[i]);
		}
		activeObjects.Clear ();
	}

	[System.Serializable]
	public class LevelChunk {
		public float chunkSize;
		public List<LevelObject> levelObjects;

		public LevelChunk (float _chunkSize, List<LevelObject> _levelObjects) {
			chunkSize = _chunkSize;
			levelObjects = _levelObjects;
		}
	}

	[System.Serializable]
	public class LevelObject {
		public Vector2 pos;
		public GameObject prefab;

		public LevelObject (Vector2 _pos, GameObject _prefab) {
			pos = _pos;
			prefab = _prefab;
		}
	}

	// level chunks
	[Header("Object Prefabs")]
	public GameObject astroid0;
	public GameObject astroid1;

	void BuildLevelChunks () {
		List<LevelObject> levelObjects = new List<LevelObject> ();

		// new chunk ID: -1
		// objects
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-7f, 4f), _prefab: astroid1));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (4f, 15f), _prefab: astroid0));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-5f, 23f), _prefab: astroid1));
		// build chunk
		allLevelChunks.Add (new LevelChunk (_chunkSize: 27, _levelObjects: new List<LevelObject> (levelObjects)));
		levelObjects.Clear ();
	}
}
