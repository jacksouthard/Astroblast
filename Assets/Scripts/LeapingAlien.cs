﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeapingAlien : BasicAlien {
	public float leapForce;
	public float leapDelay;
	public float acceleration;
	public float maxSpeed;
	public float rotationSpeed;

	protected override void Initiated () { // called by master on start

	}

	protected override void Awaken () {
		transform.Find ("Eye").gameObject.SetActive (true);
		base.Awaken (); // plays awaken animation
	}

    protected override void AlienUpdate () {
		if (state == State.awakening) {
			leapDelay -= Time.deltaTime;
			if (leapDelay <= 0f) {
				Leap ();
			}
		} else if (state == State.attacking && active) {
			MoveTowardsTarget ();
		}

        base.AlienUpdate();
	}

	void MoveTowardsTarget () {
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float angleToTarget = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;

		// rotate towards target
        Quaternion toRotation = Quaternion.Euler(0, 0, angleToTarget - 90f); //"fowards" is up
		transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

		// add force if not traveling max speed
		if (rb.velocity.magnitude < maxSpeed) {
			rb.AddRelativeForce (Vector2.up * acceleration * Time.deltaTime);
		}
	}

	void Leap () {
		rb.isKinematic = false;
		rb.AddRelativeForce (Vector2.up * leapForce);

		state = State.attacking;
		anim.SetTrigger ("Attack");
	}
}
