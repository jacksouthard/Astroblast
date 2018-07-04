using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour {
	bool flickering = false;
	bool active = false;
	public float activeTime;
	public float offTime;
	float timer;

	Light l;

	void Start () {
		l = GetComponent<Light> ();
		l.enabled = false;
	}

	public void StartFlicker () {
		flickering = true;
		active = true;
		l.enabled = true;
		timer = activeTime;
	}

	public void StopFlicker () {
		flickering = false;
		l.enabled = false;
	}
	
	void Update () {
		if (flickering) {
			timer -= Time.deltaTime;
			if (timer <= 0f) {
				if (active) { // needs to turn off
					l.enabled = false;
					active = false;
					timer = offTime;
				} else { // needs to turn on
					l.enabled = true;
					active = true;
					timer = activeTime;
				}
			}
		}
	}
}
