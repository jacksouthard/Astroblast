using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFlicker : MonoBehaviour {
	bool flickering = false;
	bool active = false;
	public float activeTime;
	public float offTime;
	Color startColor;
	public Color flickerColor;
	float timer;

	Text text;

	void Start () {
		text = GetComponent<Text> ();
		startColor = text.color;
	}

	public void StartFlicker () {
		flickering = true;
		active = true;
		timer = activeTime;
	}

	public void StopFlicker () {
		flickering = false;
		text.color = startColor;
	}

	void Update () {
		if (flickering) {
			timer -= Time.deltaTime;
			if (timer <= 0f) {
				if (active) { // needs to turn to start color
					active = false;
					text.color = startColor;
					timer = offTime;
				} else { // needs to turn to flicker color
					active = true;
					text.color = flickerColor;
					timer = activeTime;
				}
			}
		}
	}
}
