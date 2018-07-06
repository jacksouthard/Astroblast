using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

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
	public int maxWallObjects;
	public float wallObjectAngleVariance;
	public float wallObjectMinSpacing;
	public float wallObjectSpawnRange;
	public float wallObjectZ;

	[Header("Side Pieces")]
	public GameObject[] sidePiecePrefabs;
	public float sidePieceX;
	public float sidePieceSpawnIntervals;
	public float initialSidePieceCount;
	public float initialSidePieceY;
	Transform lastSidePiece = null;

	[HideInInspector]
	public float farthestY = 0f;
	Transform persistantObjects;
	Transform mainCam;
	float curChunkY = 0f;
	float objectActiveRange = 5f;

    [Header("UI")]
    public Text depthText;
    public Transform depthArrow;

	// difficulty and scaling
	bool allTeirsActive = false;
	int curTeirIndex = 0;
	public int curDifficulty;
	Astroid curAstroid;

	void Awake () {
		instance = this;
		BuildLevelChunks ();
        InitUI();
	}

	void Start () {
		mainCam = Camera.main.transform;
		persistantObjects = GameObject.Find ("PersistantObjects").transform;
		curAstroid = astroids [0];
		// spawn initial chunks
		ResetTerrain();
	}
		
	void Update () {
		if (shouldUpdate && mainCam.position.y * direction > farthestY * direction) {
			farthestY = mainCam.position.y;
            depthText.text = Mathf.Abs(Mathf.RoundToInt(farthestY)).ToString() + "m";

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
		//if (direction == -1) {
        direction = -direction;
        depthArrow.rotation *= Quaternion.Euler(0, 0, 180);
		Camera.main.GetComponent<CameraController> ().SwitchDirections (direction);
		//}
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

    void InitUI() {
        depthText.text = "0m";
        depthArrow.rotation = Quaternion.identity;
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
		WallObjectDifficultyData diffData = allWallObjectDifficultyData[Mathf.Clamp (Random.Range(0, curDifficulty + 1), 0, allWallObjectData.Length - 1)];
		List<float> usedHeights = new List<float> ();

		int objectCount = Random.Range (0, maxWallObjects + 1);
		for (int i = 0; i < objectCount; i++) {
			WallObjectData objData = allWallObjectData[diffData.objectIndexes [Random.Range (0, diffData.objectIndexes.Length)]];	

			// find avaiable spawn height
			float spawnY = Random.Range (-wallObjectSpawnRange, wallObjectSpawnRange);
			if (usedHeights.Count != 0) {
				// if is not the first object, check to make sure it does not overlap with other objects
				int attemps = 0;
				while (WallObjectSpawnInvalid(spawnY, usedHeights) && attemps <= 5) {
					spawnY = Random.Range (-wallObjectSpawnRange, wallObjectSpawnRange);
					attemps++;
				}
			}
			usedHeights.Add (spawnY);

			// spawn object
			Vector3 spawnPos = new Vector3 (sidePieceX * sideSign, anchorY + spawnY, wallObjectZ);

			float size = Random.Range (objData.minScale, objData.maxScale);
			Vector3 scale = new Vector3 (size, size, 1f);

			float xRot = 0f;
			if (Random.value > 0.5f) {
				xRot = 180f;
			}
			Quaternion spawnRot = Quaternion.Euler (xRot, 0f, (sideSign * 90f) + Random.Range (-wallObjectAngleVariance, wallObjectAngleVariance));
			GameObject spawn = Instantiate (objData.prefab, spawnPos, spawnRot, persistantObjects);
			spawn.transform.localScale = scale;
		}
	}

	bool WallObjectSpawnInvalid (float newHeight, List<float> usedHeights) {
		foreach (float height in usedHeights) {
			if (Mathf.Abs (height - newHeight) < wallObjectMinSpacing) {
				return true;
			}
		}
		return false;
	}

	void ExpressNextChunk () {
		if (!allTeirsActive && curChunkY < curAstroid.teirs[curTeirIndex + 1].appearsAfter) {
			// increase to next teir
			curTeirIndex++;
			curDifficulty = curAstroid.teirs [curTeirIndex].difficulty;
			print ("Moved to teir " + curTeirIndex + " with " + curDifficulty + " difficulty");
			if (curTeirIndex >= curAstroid.teirs.Length - 1) {
				allTeirsActive = true;
				print ("All teirs now active");
			}

		}
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
		int randTeirID = Random.Range (0, curTeirIndex + 1);
		int[] chunkIDs = curAstroid.teirs [randTeirID].chunkIDs;

		int randChunkID = chunkIDs[Random.Range (0, chunkIDs.Length)];
		return allLevelChunks [randChunkID];
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

	// astroids
	[Header("Astroids")]
	public Astroid[] astroids;

	[System.Serializable]
	public class Astroid { 
		public string name;
		public Teir[] teirs;
	}

	// teirs
	[System.Serializable]
	public class Teir {
		public int difficulty;
		public float appearsAfter;
		public int[] chunkIDs;
	}

	// chunks
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

	[Header("Object Prefabs")]
	public GameObject smoothRock;
	public GameObject spikyRock;
	public GameObject crystal;

	void BuildLevelChunks () {
		List<LevelObject> levelObjects = new List<LevelObject> ();

		// INDEX 0 basic smooth rock formation
		// objects
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-7f, -4f), _prefab: smoothRock));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (4f, -13f), _prefab: smoothRock));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-5f, -19f), _prefab: smoothRock));
		// build chunk
		allLevelChunks.Add (new LevelChunk (_chunkSize: 23, _levelObjects: new List<LevelObject> (levelObjects)));
		levelObjects.Clear ();

		// INDEX 1 basic spiky rock formation
		// objects
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-5f, -2f), _prefab: spikyRock));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (6f, -11f), _prefab: spikyRock));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-5f, -13f), _prefab: spikyRock));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-1f, -21f), _prefab: smoothRock));
		// build chunk
		allLevelChunks.Add (new LevelChunk (_chunkSize: 26, _levelObjects: new List<LevelObject> (levelObjects)));
		levelObjects.Clear ();

		// INDEX 2 basic spiky rock / smooth rock formation
		// objects
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-1f, -7f), _prefab: spikyRock));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (8f, -8f), _prefab: smoothRock));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (6f, -17f), _prefab: spikyRock));
		levelObjects.Add (new LevelObject (_pos: new Vector2 (-7f, -18f), _prefab: smoothRock));
		// build chunk
		allLevelChunks.Add (new LevelChunk (_chunkSize: 24, _levelObjects: new List<LevelObject> (levelObjects)));
		levelObjects.Clear ();
	}

	// wall objects
	public WallObjectDifficultyData[] allWallObjectDifficultyData;
	[System.Serializable]
	public class WallObjectDifficultyData {
		public int[] objectIndexes;
	}

	public WallObjectData[] allWallObjectData;
	[System.Serializable]
	public class WallObjectData {
		public GameObject prefab;
		public float minScale;
		public float maxScale;
	}
}
