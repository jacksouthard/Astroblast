using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienEgg : BasicAlien {
	public GameObject hatchling;
	public float smallSize;
	public float largeSize;
	bool armed = false;

	protected override void Initiated () { // called by master on start
		state = State.awakened;
		transform.localScale = new Vector3 (smallSize, smallSize, 1f);
	}

	protected override void Activate () {
		if (TerrainManager.instance.direction == 1) {
			armed = true;
			transform.localScale = new Vector3 (largeSize, largeSize, 1f);
		}
		base.Activate ();
	}
	protected override void TripAttack () {
		if (armed) {
			anim.SetTrigger ("Hatch");
		}
	}
		
	void SpawnHatchling () {
		GameObject newAlien = Instantiate (hatchling, transform.position, transform.rotation, transform.parent);
		newAlien.GetComponent<BasicAlien> ().InitPos (curAstroid, curIndex);
	}

	void HatchingComplete () {
		Destroy (gameObject);
	}
}
