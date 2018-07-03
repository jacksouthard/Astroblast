using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public float baseFollowSpeed = 2f;
	float curFollowSpeed;
	float farthestY = 0f;
	Transform target;
	Transform background;

	// slowing to stop
	public float stopTime;
	float stopTimer;
	float targetFollowSpeed = 0.05f;
	bool frozen = false;
	bool stopping = false;

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
	}

	void LateUpdate () {
		if (!frozen) {
			if (target.position.y > farthestY) {
				farthestY = target.position.y;
			}
			MoveCam ();
		}
	}

	void MoveCam () {
		Vector3 newPosition = new Vector3 (target.position.x, farthestY, transform.position.z);
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
}
