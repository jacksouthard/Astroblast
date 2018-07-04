using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour {
	public static ShipController instance;

	public bool skipEntry;
	public bool canExit;
	Animator anim;
	Transform player;
	CameraController camCon;

	void Awake () {
		instance = this;
	}

	void Start () {
		camCon = Camera.main.GetComponent<CameraController> ();
		camCon.SetZoom (22f);

		anim = GetComponent<Animator> ();

		if (skipEntry) {
			anim.SetTrigger ("Hover");
			EnterHover ();
		} else {
			anim.SetTrigger ("Enter");
//			camCon.StartTracking (transform);
		}
	}

	void Update () {
		if (canExit && Input.GetKeyDown (KeyCode.Space)) {
			// temp for exiting ship
			PlayerExit();
		}
	}

	void EnterHover () { // called partially as an event in the enter animation
		canExit = true;
	}

	void ExitHover () {
		canExit = false;
	}

	void OnCollisionEnter2D (Collision2D coll) {
		if (coll.gameObject.tag == "Player") {
			if (TerrainManager.instance.direction == 1) {
				PlayerEnter (player);
//				camCon.StartTracking (transform);
				camCon.StartZoom (22f, 2f);
				anim.SetTrigger ("Exit");
			}
		}
	}

	// player relations
	public void PlayerEnter (Transform _player) {
		player = _player;
		camCon.StopTracking();
		player.gameObject.SetActive (false);
	}

	public void PlayerExit () {
		player.gameObject.SetActive (true);
		player.position = transform.Find ("PlayerSpawn").position;
		camCon.StartTracking (player);
		camCon.StartZoom (16f, 2f);
		canExit = false;
	}
}
