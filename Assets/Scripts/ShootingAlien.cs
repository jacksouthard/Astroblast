using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingAlien : BasicAlien {
	public GameObject projectile;
	public bool destroyBarbOnDeath;
	Barb curBarb;
	public float shotSpeed;
	public float awakenTime;
	public float attackCooldown;
	float cooldownTimer;
	bool onCooldown = false;

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

		if (onCooldown) {
			cooldownTimer -= Time.deltaTime;
			if (cooldownTimer <= 0f) {
				onCooldown = false;
			}
		}

        base.AlienUpdate();
	}

	protected override void TripAttack () {
		if (!onCooldown) {
			StartAttacking ();
		}
	}

	protected override void PhysicalTripAttack () {
		// for barbalien to close and munch on player
		anim.SetTrigger ("Bite");
	}


	void StartAttacking () {
		state = State.attacking;
		anim.SetTrigger ("Attack");

		// start cooldown
		if (attackCooldown != 0) {
			cooldownTimer = attackCooldown;
			onCooldown = true;
		}
	}

	void Shoot () { // called in attack animation
		GameObject newProjectile = Instantiate (projectile, bulletSpawn.position, bulletSpawn.rotation, transform.parent);
		newProjectile.GetComponent<Rigidbody2D> ().AddRelativeForce (Vector2.up * shotSpeed);
		newProjectile.GetComponent<AlienProjectile> ().parentAlien = this;
		if (destroyBarbOnDeath) {
			curBarb = newProjectile.GetComponent<Barb> ();
		}
	}

	void AttackComplete () { // called when attack animation is done in animation
		state = State.awakened;
	}

	protected override void Die () {
		if (destroyBarbOnDeath && curBarb != null) {
			curBarb.BarbBreak ();
		}
		base.Die ();
	}
		
}
