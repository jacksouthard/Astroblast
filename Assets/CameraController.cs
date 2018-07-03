using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public float followSpeed = 2f;
	float farthestY = 0f;
	Transform target;
	Transform background;

	void Start () {
		target = GameObject.Find ("Player").transform;
		background = GameObject.Find ("Background").transform;
	}

	void LateUpdate () {
		if (target.position.y > farthestY) {
			farthestY = target.position.y;
		}
		MoveCam ();
	}

	void MoveCam () {
		Vector3 newPosition = new Vector3 (target.position.x, farthestY, transform.position.z);
		transform.position = Vector3.Slerp(transform.position, newPosition, followSpeed * Time.deltaTime);

		background.position = new Vector3 (0f, transform.position.y, background.position.z);
	}
}
