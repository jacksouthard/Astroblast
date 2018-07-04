using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour {
	public bool skipEntry;
	public bool playerCanEject;
	Animator anim;
	Transform player;

	void Start () {
		anim = GetComponent<Animator> ();

		if (skipEntry) {
			anim.SetTrigger ("Hover");
			EnterHover ();
		} else {
			anim.SetTrigger ("Enter");
		}
	}

	void EnterHover () { // called partially as an event in the enter animation
		playerCanEject = true;
	}

	void ExitHover () {
		playerCanEject = false;
	}

	// player relations
	void PlayerEnter (Transform player) {
		player.gameObject.SetActive (false);
	}

	void PlayerExit () {
		player.gameObject.SetActive (true);
		player.position = transform.Find ("PlayerSpawn").position;
	}
}
