using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public float followSpeed = 2f;
	float farthestY = 0f;
	Transform target;

	void Start () {
		target = GameObject.Find ("Player").transform;
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
	}
}
