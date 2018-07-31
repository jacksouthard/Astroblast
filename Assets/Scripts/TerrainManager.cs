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
	// bottom
	public float bottomBuffer;
	[HideInInspector]
	public float maxDepth;
	public GameObject bottom;

	[Header("Wall Objects")]
	public int maxWallObjects;
	public float wallObjectAngleVariance;
	public float wallObjectMinSpacing;
	public float wallObjectSpawnRange;
	public float wallObjectZ;

	[Header("Side Pieces")]
//	public List<int> activePieces = new List<int>();
	public GameObject[] sidePiecePrefabs;
	public float sidePieceX;
	public float sidePieceSpawnIntervals;
	public float initialSidePieceCount;
	public float initialSidePieceY;
	float? lastSidePieceY = null;

	[HideInInspector]
	public float currentY = 0f;
	float farthestY = 0f;
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
	public int baseDifficulty;
	Astroid curAstroid;

	void Awake () {
		instance = this;
		BuildLevelChunks ();
        InitUI();
	}

	void Start () {
		mainCam = Camera.main.transform;
		persistantObjects = GameObject.Find ("PersistantObjects").transform;
		curAstroid = astroids [curAstroidIndex];
		baseDifficulty = curAstroid.baseDifficulty;
		maxDepth = curAstroid.depth;

		float bottomSpawnY = -maxDepth + (maxDepth % sidePieceSpawnIntervals);
		Instantiate (bottom, new Vector3 (0f, bottomSpawnY, 0f), Quaternion.identity);
		// spawn initial chunks
		ResetTerrain();
	}
		
	void Update () {
		if (shouldUpdate && mainCam.position.y < currentY) {
			farthestY = mainCam.position.y;
		}

		if (shouldUpdate && mainCam.position.y * direction > currentY * direction) {
			currentY = mainCam.position.y;
			if (currentY <= -maxDepth + bottomBuffer) {
				BottomController.instance.AttemptExpression ();
			}
            depthText.text = Mathf.Abs(Mathf.RoundToInt(currentY)).ToString() + "m";

			if (spawnTerrain && direction == -1) { // chunks only spawn when going into astroid
				if (currentY - objectSpawnBuffer < curChunkY && currentY - objectSpawnBuffer > -(maxDepth - bottomBuffer)) {
					ExpressNextChunk ();
				}
			}
				
			if (currentY < initialSidePieceY && direction == -1) { // dont spawn side pieces above extraction point
				if (lastSidePieceY == null) {
					CreateNextSidePiece ();
				} else if ((currentY + (objectSpawnBuffer * direction)) * direction > lastSidePieceY.Value * direction) {
					CreateNextSidePiece ();
				}
			}

//			TestForObjectDespawns ();
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
		currentY = 0f;
		curChunkY = 0f;

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
		if (lastSidePieceY == null) {
			spawnPos = new Vector3 (sidePieceX, initialSidePieceY, 0f);
		} else {
			spawnPos = new Vector3 (sidePieceX, lastSidePieceY.Value + (sidePieceSpawnIntervals * direction), 0f);
			if (spawnPos.y > initialSidePieceY || spawnPos.y < -maxDepth + (sidePieceSpawnIntervals / 2f)) {
//				print ("Tried to spawn side piece in extraction area");
				return;
			}
		}

		// check to make sure this location is avaiable for a side piece
//		int roundedY = Mathf.RoundToInt(spawnPos.y);
//		if (activePieces.Contains (roundedY)) {
//			print ("loc in use");
//			return;
//		}
//		activePieces.Add (roundedY);
		lastSidePieceY = spawnPos.y;

		GameObject prefab = sidePiecePrefabs [Random.Range (0, sidePiecePrefabs.Length)];

		for (int sign = -1; sign <= 1; sign += 2) {

			Vector3 newSpawnPos = new Vector3 (sidePieceX * sign, spawnPos.y, spawnPos.z);
			GameObject piece = (GameObject)Instantiate (prefab, newSpawnPos, Quaternion.identity, transform);

			piece.transform.localScale = new Vector3 (piece.transform.localScale.x * -sign, piece.transform.localScale.y, 1f);

			// wall objects are persistant so only generated when going down
			if (direction == -1) {
				CreateWallObjects (spawnPos.y, sign);
			}
		}
	}

	void CreateWallObjects (float anchorY, int sideSign) {
		WallObjectDifficultyData diffData = allWallObjectDifficultyData[Mathf.Clamp (Random.Range(0, curDifficulty + 1 + baseDifficulty), 0, allWallObjectData.Length - 1)];
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
			print ("Moved to teir " + curTeirIndex + " with " + (curDifficulty + baseDifficulty) + " difficulty");
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
		float despawnMargin = currentY - (objectDespawnDst * direction);
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
//			if (GO.tag == "Rock") { // must be side piece
//				activePieces.Remove (Mathf.RoundToInt(GO.transform.position.y));
//			}
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
	public int curAstroidIndex;
	public Astroid[] astroids;

	[System.Serializable]
	public class Astroid { 
		public string name;
		public int baseDifficulty;
		public float depth;
		public Teir[] teirs;
	}

	public string GetAstroidNameFromIndex (int index) {
		return astroids [index].name;
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
