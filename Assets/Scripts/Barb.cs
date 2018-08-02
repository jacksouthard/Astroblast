using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barb : AlienProjectile {
	public float lifeTime;
	public float lifeTimeAfterHit;
	public float reelForce;
	public float dragOnHit;
	bool embeded = false;
	bool hitPlayer = false;

	Transform player;
	Transform tip;
	Vector3 barbAnchor;
	LineRenderer lr;

	void Start () {
		lr = GetComponent<LineRenderer> ();
		lr.startColor = parentAlien.difficultyColor;
		lr.endColor = parentAlien.difficultyColor;

		tip = transform.GetChild (0);
		barbAnchor = transform.position;
	}

	void Update () {
		lifeTime -= Time.deltaTime;
		if (lifeTime <= 0f) {
			BarbBreak ();
		}

		if (hitPlayer) {
			transform.position = new Vector3 (player.position.x, player.position.y, transform.position.z);
			Vector2 playerPos2d = new Vector2 (player.position.x, player.position.y);
			Vector2 pos2d = new Vector2 (barbAnchor.x, barbAnchor.y);
			Vector2 dir = (playerPos2d - pos2d).normalized;
			player.GetComponent<Rigidbody2D> ().AddForce (-dir * reelForce * Time.deltaTime);
		}

		lr.SetPosition (0, barbAnchor);
		lr.SetPosition (1, transform.position);
	}

	void OnTriggerEnter2D (Collider2D coll) {
		if (!embeded) {
			if (coll.gameObject.tag == "Player") {
				Embed (coll.transform, true);
			} else if (coll.gameObject.tag == "Rock") {
				Embed (coll.transform, false);
			}
		}
	}

	void Embed (Transform target, bool _hitPlayer) {
		hitPlayer = _hitPlayer;
		if (hitPlayer) {
			player = target;
			player.GetComponent<Rigidbody2D> ().drag = dragOnHit;
		}
		lifeTime = lifeTimeAfterHit;
		Destroy (GetComponent<Rigidbody2D> ());
		tip.parent = target;
		embeded = true;
	}

	public void BarbBreak () {
		if (embeded) {
			Destroy (tip.gameObject);
		}
		if (hitPlayer) {
			player.GetComponent<Rigidbody2D> ().drag = 0f;
		}

		Destroy (gameObject);
	}
}
