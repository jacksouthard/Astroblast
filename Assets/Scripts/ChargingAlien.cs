using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingAlien : LeapingAlien {
	public float chargeForce;
	bool charging = false;
	float awakenedTriggerWidth = 2f;

	protected override void Initiated () { // called by master on start

	}

	protected override void Awaken () {
		transform.Find ("Eye").gameObject.SetActive (true);
		BoxCollider2D boxColl = GetComponent<BoxCollider2D> ();
		boxColl.size = new Vector2 (awakenedTriggerWidth, boxColl.size.y); // make it more accurate when awakened
		base.Awaken (); // plays awaken animation
	}
		
	protected override void TripAttack () {
		StartAttacking ();
	}

	void StartAttacking () {
		state = State.attacking;
		anim.SetTrigger ("Attack");
	}

	void Charge () { // called in attack animation
		rb.AddRelativeForce (Vector2.up * chargeForce);
		charging = true;
	}

	void AttackComplete () { // called when attack animation is done in animation
		state = State.awakened;
		charging = false;
	}
}
