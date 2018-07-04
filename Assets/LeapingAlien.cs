using System.Collections;
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

	void Update () {
		if (state == State.awakening) {
			leapDelay -= Time.deltaTime;
			if (leapDelay <= 0f) {
				Leap ();
			}
		} else if (state == State.attacking && active) {
			MoveTowardsTarget ();
		}
	}

	void MoveTowardsTarget () {
		Vector3 dirToTarget = (target.position - transform.position).normalized;
		dirToTarget = new Vector3 (dirToTarget.x, dirToTarget.y, 0f);

		// rotate towards target
		Quaternion toRotation = Quaternion.LookRotation(transform.forward, dirToTarget);
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
