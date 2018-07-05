using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TerrainManager : MonoBehaviour {
	public static TerrainManager instance;

	public bool spawnTerrain;
	bool shouldUpdate = true;
	public int direction = -1; // -1 of going down, 1 if going up

	[Header("Chunks")]
	public float initalChunkY;
	public float objectSpawnBuffer;
	public float objectDespawnDst;
	List<LevelChunk> allLevelChunks = new List<LevelChunk>();

	[Header("Wall Objects")]
	public GameObject[] wallPrefabs;
	public int maxWallObjects;
	public float wallObjectAngleVariance;
	public float wallObjectSpawnRange;
	public float wallObjectZ;

	[Header("Side Pieces")]
	public GameObject[] sidePiecePrefabs;
	public float sidePieceX;
	public float sidePieceSpawnIntervals;
	public float initialSidePieceCount;
	public float initialSidePieceY;
	Transform lastSidePiece = null;

//	[HideInInspector]
	public float farthestY = 0f;
	Transform persistantObjects;
	Transform mainCam;
	float curChunkY = 0f;
	float objectActiveRange = 5f;

	void Awake () {
		instance = this;
		BuildLevelChunks ();
	}

	void Start () {
		mainCam = Camera.main.transform;
		persistantObjects = GameObject.Find ("PersistantObjects").transform;
		// spawn initial chunks
		ResetTerrain();
	}
		
	void Update () {
		if (shouldUpdate && mainCam.position.y * direction > farthestY * direction) {
			farthestY = mainCam.position.y;

			if (spawnTerrain && direction == -1) { // chunks only spawn when going into astroid
				if (farthestY - objectSpawnBuffer < curChunkY) {
					ExpressNextChunk ();
				}
			}
				
			// side pieces are generated dynamically based on where the player is
			if (farthestY < initialSidePieceY) { // dont spawn side pieces above extraction point
				if (lastSidePiece == null) {
					CreateNextSidePiece ();
				} else if ((farthestY + (objectSpawnBuffer * direction)) * direction > lastSidePiece.position.y * direction) {
					CreateNextSidePiece ();
				}
			}

			TestForObjectDespawns ();
		}

		if (Input.GetKeyDown (KeyCode.F)) { // temp
			SwitchDirections();
		}
	}

	public void StopUpdating () {
		shouldUpdate = false;
	}

	public void SwitchDirections () {
		// only switch if going down
		if (direction == -1) {
			direction = 1;
			Camera.main.GetComponent<CameraController> ().SwitchDirections (direction);
		}
	}

	public void ResetTerrain () {
		farthestY = 0f;
		curChunkY = 0f;

		if (spawnTerrain) {
//			ExpressNextChunk ();
		}

//		for (int i = 0; i < initialBackgroundCount; i++) {
//			CreateNextBackground ();
//		}

		for (int i = 0; i < initialSidePieceCount; i++) {
			CreateNextSidePiece ();
		}
	}

	void CreateNextSidePiece () {
		Vector3 spawnPos;
		if (lastSidePiece == null) {
			spawnPos = new Vector3 (sidePieceX, initialSidePieceY, 0f);
		} else {
			spawnPos = new Vector3 (sidePieceX, lastSidePiece.position.y + (sidePieceSpawnIntervals * direction), 0f);
			if (spawnPos.y > initialSidePieceY) {
//				print ("Tried to spawn side piece in extraction area");
				return;
			}
		}

		GameObject prefab = sidePiecePrefabs [Random.Range (0, sidePiecePrefabs.Length)];

		for (int sign = -1; sign <= 1; sign += 2) {

			Vector3 newSpawnPos = new Vector3 (sidePieceX * sign, spawnPos.y, spawnPos.z);
			GameObject piece = (GameObject)Instantiate (prefab, newSpawnPos, Quaternion.identity, transform);
			lastSidePiece = piece.transform;

			piece.transform.localScale = new Vector3 (piece.transform.localScale.x * -sign, piece.transform.localScale.y, 1f);

			// wall objects are persistant so only generated when going down
			if (direction == -1) {
				CreateWallObjects (spawnPos.y, sign);
			}
		}
	}

	void CreateWallObjects (float anchorY, int sideSign) {
		int objectCount = Random.Range (0, maxWallObjects + 1);
		for (int i = 0; i < objectCount; i++) {
			GameObject prefab = wallPrefabs [Random.Range (0, wallPrefabs.Length)];
			Vector3 spawnPos = new Vector3 (sidePieceX * sideSign, anchorY + Random.Range (-wallObjectSpawnRange, wallObjectSpawnRange), wallObjectZ);

			float yRot = 0f;
			if (Random.value > 0.5f) {
				yRot = 180f;
			}
			Quaternion spawnRot = Quaternion.Euler (0f, yRot, (sideSign * 90f) + Random.Range (-wallObjectAngleVariance, wallObjectAngleVariance));
			Instantiate (prefab, spawnPos, spawnRot, persistantObjects);
		}
	}

	void ExpressNextChunk () {
		LevelChunk chunk = GetChunk ();
		int xFlipSign = 1;
		if (Random.value > 0.5f) {
			xFlipSign = -1;
		}
	
		foreach (var levelObject in chunk.levelObjects) {
			Vector3 spawnPos = new Vector3 (levelObject.pos.x * xFlipSign, levelObject.pos.y + curChunkY + initalChunkY, 1f);
			Instantiate (levelObject.prefab, spawnPos, Quaternion.identity, transform);
		}

		curChunkY -= chunk.chunkSize;
	}

	LevelChunk GetChunk () {
		int random = Random.Range (0, allLevelChunks.Count);

		return allLevelChunks [random];
	}

	void TestForObjectDespawns () {
		float despawnMargin = farthestY - (objectDespawnDst * direction);
		List<GameObject> objectsToDelete = new List<GameObject> ();
		for (int i = 0; i < transform.childCount; i++) {
			Transform child = transform.GetChild (i);
			if (child != null) {
				if (child.position.y * direction < despawnMargin * direction) {
					objectsToDelete.Add (child.gameObject);
				}
			}
		}
		foreach (var GO in objectsToDelete) {
			Destroy (GO);

		}
			
	}

	public bool ShouldBeActive (float y) {
		if (Mathf.Abs (y - mainCam.position.y) < objectActiveRange) {
			return true;
		} else {
			return false;
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
	public GameObject smoothAstroid;
	public GameObject spikyAstroid;
	public GameObject crystal;

	void BuildLevelChunks () {
		List<LevelObject> levelObjects = new List<LevelObject> ();

		// objects
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-7f, -4f), _prefab: smoothAstroid));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (4f, -13f), _prefab: smoothAstroid));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-5f, -19f), _prefab: smoothAstroid));
		// build chunk
		allLevelChunks.Add (new LevelChunk (_chunkSize: 23, _levelObjects: new List<LevelObject> (levelObjects)));
		levelObjects.Clear ();

		// objects
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-5f, -2f), _prefab: spikyAstroid));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (6f, -11f), _prefab: spikyAstroid));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-5f, -13f), _prefab: spikyAstroid));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-1f, -21f), _prefab: smoothAstroid));
		// build chunk
		allLevelChunks.Add (new LevelChunk (_chunkSize: 26, _levelObjects: new List<LevelObject> (levelObjects)));
		levelObjects.Clear ();
	}
}
