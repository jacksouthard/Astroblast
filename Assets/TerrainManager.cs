using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TerrainManager : MonoBehaviour {
	public static TerrainManager instance;

	public bool spawnTerrain;
	bool shouldUpdate = true;

	[Header("Chunks")]
	public float objectSpawnBuffer;
	public float objectDespawnDst;
	List<LevelChunk> allLevelChunks = new List<LevelChunk>();

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
	public float crystalHeightRange;
	Transform lastSidePiece = null;

	[HideInInspector]
	public float farthestY = 0f;
	Transform mainCam;
	float curChunkY = 0f;

	void Awake () {
		instance = this;
		BuildLevelChunks ();
	}

	void Start () {
		mainCam = Camera.main.transform;

		maxTeir = Mathf.FloorToInt (allLevelChunks.Count / chunksPerTeir);

		// spawn initial chunks
		ResetTerrain();
	}
		
	void Update () {
		if (mainCam.transform.position.y > farthestY && shouldUpdate) {
			farthestY = mainCam.transform.position.y;

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

	public void StopUpdating () {
		shouldUpdate = false;
	}

	public void ResetTerrain () {
		ClearObjects ();

//		initialStarX += player.position.y;
		initialBackgroundY += mainCam.position.y;
		initialSidePieceY += mainCam.position.y;

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

			int crystalCount = Random.Range (0, 2);
			for (int i = 0; i < crystalCount; i++) {
				Vector3 crystalSpawnPos = new Vector3 (newSpawnPos.x, newSpawnPos.y + Random.Range (-crystalHeightRange, crystalHeightRange), newSpawnPos.z + 0.1f);
				Quaternion spawnRot = Quaternion.Euler (0f, 0f, sign * 90f);
				GameObject newCrystal = Instantiate (crystal, crystalSpawnPos, spawnRot, transform);
			}
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
			GameObject GO = (GameObject)Instantiate (levelObject.prefab, spawnPos, Quaternion.identity, transform);
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
		for (int i = 0; i < transform.childCount; i++) {
			Transform child = transform.GetChild (i);
			if (child != null) {
				if (child.position.y < despawnMargin) {
					objectsToDelete.Add (child.gameObject);
				}
			}
		}
		foreach (var GO in objectsToDelete) {
			Destroy (GO);

		}
			
	}

	void SetSeed (int _seed) {
		Random.InitState (_seed);
	}

	void ClearObjects () {
		for (int i = 0; i < transform.childCount; i++) {
			Destroy (transform.GetChild(i));
		}
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
	public GameObject astroid;
	public GameObject crystal;

	void BuildLevelChunks () {
		List<LevelObject> levelObjects = new List<LevelObject> ();

		// new chunk ID: -1
		// objects
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-7f, 4f), _prefab: astroid));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (4f, 15f), _prefab: astroid));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-5f, 23f), _prefab: astroid));
		// build chunk
		allLevelChunks.Add (new LevelChunk (_chunkSize: 27, _levelObjects: new List<LevelObject> (levelObjects)));
		levelObjects.Clear ();
	}
}
