using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TerrainManager : MonoBehaviour {
	public static TerrainManager instance;

	public bool spawnTerrain;
	bool shouldUpdate = true;
	int direction = 1; // -1 of going down, 1 if going up

	[Header("Chunks")]
	public float objectSpawnBuffer;
	public float objectDespawnDst;
	List<LevelChunk> allLevelChunks = new List<LevelChunk>();

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
	public float extractionY;
	Transform mainCam;
	float curChunkY = 0f;
	float objectActiveRange = 5f;

	void Awake () {
		instance = this;
		BuildLevelChunks ();
	}

	void Start () {
		mainCam = Camera.main.transform;

		// spawn initial chunks
		ResetTerrain();
	}
		
	void Update () {
		bool update = false;
		if (mainCam.transform.position.y * direction > farthestY * direction) {
			update = true;
		}

		if (update && shouldUpdate) {
			farthestY = mainCam.transform.position.y;

			if (spawnTerrain && direction == 1) { // chunks only spawn when going into astroid
				if (farthestY + objectSpawnBuffer > curChunkY) {
					ExpressNextChunk ();
				}
			}

//			if (lastBackground == null) {
//				CreateNextBackground ();
//			} else if (farthestY + objectSpawnBuffer > lastBackground.position.y) {
//				CreateNextBackground ();
//			}

			// side pieces are generated dynamically based on where the player is
			if (direction == -1 && farthestY < extractionY) { // dont spawn side pieces below(?) extraction point

			} else {
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
		if (direction != 1) { // only flip if going into astroid
			return;
		}
		direction = -1;

		mainCam.GetComponent<CameraController> ().StartFlip ();
	}

	public void ResetTerrain () {
		initialBackgroundY += mainCam.position.y;
		initialSidePieceY += mainCam.position.y;

		farthestY = 0f;
		curChunkY = 0f;

		if (spawnTerrain) {
			for (int i = 0; i < 3; i++) {
				ExpressNextChunk ();
			}
		}

		for (int i = 0; i < initialBackgroundCount; i++) {
			CreateNextBackground ();
		}

		for (int i = 0; i < initialSidePieceCount; i++) {
			CreateNextSidePiece ();
		}
	}

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
			spawnPos = new Vector3 (sidePieceX, lastSidePiece.position.y + (sidePieceSpawnIntervals * direction), 0f);
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
