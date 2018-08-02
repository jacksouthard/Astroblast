using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : AlienProjectile {
	public float lifeTime;
	public float impactForce;
	bool embeded = false;
	SpriteRenderer trail;

	void Start () {
		trail = transform.Find ("Trail").GetComponent<SpriteRenderer> ();
		trail.color = parentAlien.difficultyColor;
	}

	void Update () {
//		if (!embeded) {
			lifeTime -= Time.deltaTime;
			if (lifeTime <= 0f) {
				Destroy (gameObject);
			}
//		}
	}

	void OnTriggerEnter2D (Collider2D coll) {
		if (!embeded) {
			if (coll.gameObject.tag == "Player") {
				coll.GetComponentInParent<PlayerController> ().HitByDamagingObject (transform);
				coll.GetComponentInParent<Rigidbody2D> ().AddForce (transform.up * impactForce);
				Embed (coll.transform);
			} else if (coll.gameObject.tag == "Rock") {
				Embed (coll.transform);
			}
		}
	}

	void Embed (Transform target) {
		Destroy (GetComponent<Rigidbody2D> ());
		trail.enabled = false;
		transform.parent = target;
		embeded = true;
	}
}
