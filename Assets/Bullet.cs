using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
	public int damage;

	void OnCollisionEnter2D (Collision2D coll) {
		if (coll.gameObject.tag == "Rock") {
			Destroy (gameObject);
		}
	}

	public void Hit () { // called on aliens it hits
		Destroy (gameObject);
	}
}
