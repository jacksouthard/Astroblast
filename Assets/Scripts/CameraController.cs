using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public float baseFollowSpeed = 2f;
	float curFollowSpeed;
	float farthestY = 0f;
	public float xRange;
	Transform target;
	Transform background;

	// slowing to stop
	public float stopTime;
	float stopTimer;
	float targetFollowSpeed = 0.05f;
	bool frozen = false;
	bool stopping = false;

	// flipping
	public float flipTime;
	float flipTimer;
	bool flipping = false;
	int direction = 1;

	void Start () {
		target = GameObject.Find ("Player").transform;
		background = GameObject.Find ("Background").transform;
		curFollowSpeed = baseFollowSpeed;
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

		if (flipping) {
			flipTimer -= Time.deltaTime;
			if (flipTimer <= 0f) {
				EndFlip ();
			} else {
				float timeRatio = flipTimer / flipTime;
				float z = Mathf.Lerp (180f, 0f, timeRatio);
				transform.rotation = Quaternion.Euler (0f, 0f, z);
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
		Vector3 newPosition = new Vector3 (x, farthestY, transform.position.z);
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

	public void StartFlip () {
		flipTimer = flipTime;
		flipping = true;
		direction = -1;
	}

	void EndFlip () {
		flipping = false;
		transform.rotation = Quaternion.Euler (0f, 0f, 180f); // confirm rotation is correct
	}
}
