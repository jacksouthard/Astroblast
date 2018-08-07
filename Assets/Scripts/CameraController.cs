using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public float baseFollowSpeed = 2f;
	float curFollowSpeed;
	public float directionalOffset;
	public float farthestY;
	public float maxDepthOffset;
	float minY;
	public float xRange;
	public float maxY;
	Transform target;
	Transform background;
	Camera cam;

	// zooming
	bool zooming = false;
    [HideInInspector]
	public float baseZoom = 16;
	float zoomTime;
	float zoomTimer;
	float oldZoom;
	float targetZoom;

	// slowing to stop
	public float stopTime;
	float stopTimer;
	float targetFollowSpeed = 0.05f;
	bool frozen = true;
	bool stopping = false;

	// flipping
	public float flipTime;
	float flipTimer;
	bool flipping = false;
	int direction = -1;

	void Awake () {
		cam = GetComponent<Camera> ();
	}

	void Start () {
		background = GameObject.Find ("LightBackground").transform;
		farthestY = maxY;
		transform.position = new Vector3 (transform.position.x, maxY, transform.position.z);
		curFollowSpeed = baseFollowSpeed;
		minY = -TerrainManager.instance.maxDepth + maxDepthOffset;
	}

	public void StartTracking (Transform _target) {
		frozen = false;
		target = _target;
	}

	public void StopTracking () {
		frozen = true;
	}

	public void StartZoom (float _targetZoom, float _time) {
		targetZoom = _targetZoom;
		zoomTime = _time;
		zoomTimer = zoomTime;
		oldZoom = cam.orthographicSize;
		zooming = true;
	}

	public void SetZoom (float zoom) {
		cam.orthographicSize = zoom;
	}

	void EndZoom () {
		cam.orthographicSize = targetZoom;
		zooming = false;
	}
		
	void Update () {
		if (stopping) {
			stopTimer -= Time.deltaTime;
			if (stopTimer <= 0f) {
				StopFinished ();
			} else {
				float timeRatio = stopTimer / stopTime;
				curFollowSpeed = Mathf.Lerp (targetFollowSpeed, baseFollowSpeed, timeRatio); 
			}
		}

		if (zooming) {
			zoomTimer -= Time.deltaTime;
			if (zoomTimer <= 0f) {
				EndZoom ();
			} else {
				float timeRatio = zoomTimer / zoomTime;
				cam.orthographicSize = Mathf.Lerp (targetZoom, oldZoom, timeRatio); 
			}
		}
	}

	void LateUpdate () {
		if (!frozen) {
			if (target.position.y * direction > farthestY * direction) {
				farthestY = target.position.y;
			}
			MoveCam ();
		}
	}

	void MoveCam () {
		float x = Mathf.Clamp (target.position.x, -xRange, xRange);
		float y = Mathf.Clamp (farthestY, minY, maxY) + (directionalOffset * direction);
		Vector3 newPosition = new Vector3 (x, y, transform.position.z);
		transform.position = Vector3.Slerp(transform.position, newPosition, curFollowSpeed * Time.deltaTime);

		background.position = new Vector3 (0f, transform.position.y, background.position.z);
	}

	public void PlayerDied () {
		StartStopping ();
		Camera.main.backgroundColor = Color.red;
		Light[] allLights = FindObjectsOfType<Light> ();
		foreach (var light in allLights) {
			light.enabled = false;
		}

		SpriteRenderer[] allSprites = FindObjectsOfType<SpriteRenderer> ();
		foreach (var sprite in allSprites) {
			sprite.color = Color.black;
		}
	}

	void StartStopping () {
		stopTimer = stopTime;
		stopping = true;
	}

	void StopFinished () {
		TerrainManager.instance.StopUpdating ();
		frozen = true;
		stopping = false;
	}

	public void SwitchDirections (int newDir) {
		direction = newDir;
	}
}
