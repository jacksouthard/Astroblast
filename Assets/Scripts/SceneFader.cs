﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour {
	public static float curFadeAmount { get { return (instance != null) ? instance.fader.color.a : 0; } }

	static SceneFader instance;
	public Image fader { get; private set; }

	Color fullColor;
	Color zeroColor;

	const float fadeSpeed = 10;

	void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (instance);
		} else {
			Destroy (gameObject);
		}
	}

	public void FadeToScene(int buildIndex, Color color) {
		instance.SetColor (color);
		instance.StartCoroutine (instance.SwitchScenes(buildIndex));
	}
		
	public void Init() {
		fader = gameObject.GetComponentInChildren<Image> ();
	}

	public void SetColor(Color color) {
		fullColor = new Color(color.r, color.g, color.b, 1f);
		zeroColor = new Color(color.r, color.g, color.b, 0f);
	}

	public IEnumerator SwitchScenes(int buildIndex) {
		yield return StartCoroutine (FadeIn());

		AsyncOperation loadingLevel = SceneManager.LoadSceneAsync (buildIndex);
		yield return new WaitUntil (() => loadingLevel.isDone);

		yield return StartCoroutine (FadeOut());
	}
		
	IEnumerator FadeIn() {
		fader.raycastTarget = true;
		fader.color = zeroColor;
		float p = 0f;
		float t = Time.fixedUnscaledDeltaTime;

		while(p < 1f) {
			fader.color = Color.Lerp (zeroColor, fullColor, p);
			p += t * fadeSpeed;
			yield return new WaitForSecondsRealtime (t);
		}
		fader.color = fullColor;
	}

	IEnumerator FadeOut() {
		fader.color = fullColor;
		float p = 0f;
		float t = Time.fixedUnscaledDeltaTime;
		float speedFactor = (fullColor != Color.white) ? 1f : 0.03f;  //if it's white, fade out slower

		while(p < 1f) {
			fader.color = Color.Lerp (fullColor, zeroColor, p);
			p += t * fadeSpeed * speedFactor;
			yield return new WaitForSecondsRealtime (t);
		}
		fader.color = zeroColor;
		fader.raycastTarget = false;
	}
}
