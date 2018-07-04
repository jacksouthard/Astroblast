using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour {
	public Vector3 anchorPos;
	public float paralaxScale;

	float lastCamPosY;

	bool setup = false;

	Transform mc;

	void Start () {
		mc = Camera.main.transform;

		lastCamPosY = mc.position.y;
	}

	public void Init (Vector3 _anchorPos, float _paralaxScale) {
		anchorPos = _anchorPos;
		paralaxScale = _paralaxScale;

		setup = true;
	}

	void Update () {
		if (setup) {
//			float camDiff = anchorPos.x - mc.position.x;
//			float x = anchorPos.x - (camDiff / paralaxScale);

//			float camDiff = mc.position.x - anchorPos.x;
//			float x = camDiff + mc.position.x;
//	
//			transform.position = new Vector3 (x, anchorPos.y, anchorPos.z);

			float parallax = (lastCamPosY - mc.position.y) * paralaxScale;

			//set a target y position that is the current position plus the parallax
			float targetY = transform.position.y - parallax;
			transform.position = new Vector3 (anchorPos.x, targetY, anchorPos.z);

			lastCamPosY = mc.position.y;
		}
	}
}
