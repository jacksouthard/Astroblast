using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour {
	bool switching = false;
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			FadeToGame ();
		}
	}

	void FadeToGame () { // also called in animation
		if (!switching) {
			SceneFader.instance.FadeToScene (1, Color.black);
		}
		switching = true;
	}
}
