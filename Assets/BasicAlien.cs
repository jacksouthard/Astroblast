using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAlien : MonoBehaviour {
	protected enum State {
		idle,
		awakening,
		awakened,
		attacking
	};
	protected State state = State.idle;
	protected bool active = true;

	public int health;

	protected Transform target;
	protected Animator anim;
	protected Rigidbody2D rb;

	void Awake () {
		anim = GetComponent<Animator> ();
		rb = GetComponent<Rigidbody2D> ();
	}

	void Start () {
		Initiated ();
	}

	protected virtual void Initiated () {}

	void OnTriggerEnter2D (Collider2D coll) {
		if (coll.gameObject.tag == "Player") {
			if (state == State.idle) {
				target = coll.transform;
				Awaken ();
			} else if (state == State.awakened) {
				TripAttack (); // for shooting enemies
			}
		} else if (coll.tag == "ActiveZone") {
			if (!active) {
				Activate ();
			}
		}
	}

	void OnTriggerExit2D (Collider2D coll) {
		if (coll.tag == "ActiveZone") {
			if (active) {
				Deactivate ();
			}
		}
	}

	protected virtual void Activate () {
		active = true;
	}

	protected virtual void Deactivate () {
		active = false;
		rb.velocity = Vector2.zero;
	}

	protected virtual void TripAttack () {}

	protected virtual void Awaken () {
		anim.SetTrigger ("Awaken");
		state = State.awakening;
	}

	void OnCollisionEnter2D (Collision2D coll) {
		if (coll.gameObject.tag == "Bullet") {
			Bullet bullet = coll.gameObject.GetComponent<Bullet> ();
			bullet.Hit ();
			TakeDamage (bullet.damage);
		}
	}

	void TakeDamage (int damage) {
		health -= damage;
		if (health <= 0) {
			Die ();
		}
	}

	void Die () {
		Destroy (gameObject);
	}
}
