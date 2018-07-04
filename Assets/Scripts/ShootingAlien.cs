using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingAlien : BasicAlien {
	public GameObject projectile;
	public float shotSpeed;
	public float awakenTime;

	Transform bulletSpawn;
	public BoxCollider2D triggerColl;
	float awakenedTriggerWidth = 0.3f;

	protected override void Initiated () { // called by master on start
		bulletSpawn = transform.Find ("BulletSpawn");
	}

	protected override void Awaken () {
		transform.Find ("Eye").gameObject.SetActive (true);
		triggerColl.size = new Vector2 (awakenedTriggerWidth, triggerColl.size.y); // make it more accurate when awakened
		base.Awaken (); // plays awaken animation
	}

    protected override void AlienUpdate () {
		if (state == State.awakening) {
			awakenTime -= Time.deltaTime;
			if (awakenTime <= 0f) { // finished awakeneing
				state = State.awakened;
			}
		}

        base.AlienUpdate();
	}

	protected override void TripAttack () {
		StartAttacking ();
	}

	void StartAttacking () {
		state = State.attacking;
		anim.SetTrigger ("Attack");
	}

	void Shoot () { // called in attack animation
		GameObject newProjectile = Instantiate (projectile, bulletSpawn.position, bulletSpawn.rotation, transform.parent);
		newProjectile.GetComponent<Rigidbody2D> ().AddRelativeForce (Vector2.up * shotSpeed);
	}

	void AttackComplete () { // called when attack animation is done in animation
		state = State.awakened;
	}
}
