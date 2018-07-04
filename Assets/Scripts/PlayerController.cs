﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
	public float weaponDistance;
	Transform weaponMount;
	GunController gun;

	Rigidbody2D rb;
	float weaponMoveDstToBodyForce = 0.8f;

    public float minKillVelocity;
	public Transform[] arms;

	Vector2 lastWeaponAngle = Vector2.right;
   
	// death
	[Header("Oxygen")]
	float oxygen = 100f;
	public float baseDrain;
	public float leakDrain;
	public float hitWhileLeakingDrain;
	public Text oxygenText;

	// leaks
	bool leaking = false;
	ParticleSystem leakEffect;
	Vector2 leakDirection;
	public float leakForce;

	// death
	bool isDead;

	void Start () {
		leakEffect = transform.Find("LeakEffect").GetComponent<ParticleSystem> ();
		rb = GetComponent<Rigidbody2D> ();
		weaponMount = transform.Find ("WeaponMount");
		gun = GetComponentInChildren<GunController> ();
        isDead = false;

		EquipWeapon (0);
	}
	
	void Update () {
		if (!isDead) {
			// leaking
			float amountToRemove = baseDrain;
			if (leaking) {
				rb.AddRelativeForce (-leakDirection * leakForce * Time.deltaTime);
				amountToRemove += leakDrain;
			}
			RemoveOxygen (amountToRemove * Time.deltaTime);

			if (oxygen <= 0f) {
				Die ();
			}

			// shooting
			if (Input.GetMouseButton (0)) {
				Vector2 angleVector = Camera.main.ScreenToWorldPoint (Input.mousePosition) - transform.position;
				PositionWeapon (angleVector.normalized);
				gun.TryFire ();
			}
			if (Input.GetKeyDown (KeyCode.P)) {
				// temp for patching leaks
				StopLeak();
			}
		}
	}

	void RemoveOxygen (float value) {
		oxygen -= value;
		oxygen = Mathf.Clamp (oxygen, 0f, 100f);
		oxygenText.text = "" + (int)oxygen + "%";
	}

	void PositionWeapon (Vector2 angleVector) {
		Vector2 distanceVector = angleVector * weaponDistance;
		weaponMount.transform.position = new Vector3 (distanceVector.x + transform.position.x, distanceVector.y + transform.position.y, -1f);
		var angle = Mathf.Atan2(angleVector.y, angleVector.x) * Mathf.Rad2Deg;
		weaponMount.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

		// apply body force
		float angleDiff = Vector2.SignedAngle (lastWeaponAngle, angleVector);
		rb.AddTorque (-angleDiff * weaponMoveDstToBodyForce);

		lastWeaponAngle = angleVector;

		PositionArms (angle);
	}

	void PositionArms (float weaponAngle) {
		Vector3 newScale;
		// euler angles are from 0 to 360
		float angleDiff = transform.eulerAngles.z - weaponAngle;
		angleDiff = angleDiff % 360f;
//		print ("Player: " + transform.eulerAngles.z + " Weapon: " + weaponAngle + " Diff: " + angleDiff);

		if (Mathf.Abs (angleDiff) < 90f || angleDiff > 270f) {
			// weapon on right side
			newScale = new Vector3 (1f, 1f, 1f);
		} else {
			// weapon on left side
			newScale = new Vector3 (1f, -1f, 1f);
		}
		weaponMount.localScale = newScale;

		foreach (var arm in arms) {
			Vector3 weaponMountFlatPos = new Vector3 (weaponMount.transform.position.x, weaponMount.transform.position.y, 0f);
			Vector3 armFlatPos = new Vector3 (arm.transform.position.x, arm.transform.position.y, 0f);
			Vector3 posDiff = weaponMountFlatPos - armFlatPos;
			float distanceRatio = posDiff.magnitude / weaponDistance;
			arm.localScale = new Vector3 (distanceRatio, newScale.y, 1f);
			var angle = Mathf.Atan2(posDiff.y, posDiff.x) * Mathf.Rad2Deg;
			arm.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}
	}

	public void ProjectileFire (float knockback) {
		rb.AddForce (lastWeaponAngle * -knockback);
	}

    void OnCollisionEnter2D(Collision2D other) {
		bool hitDamagingObject = false;
		if (other.gameObject.tag == "Rock" && other.relativeVelocity.magnitude > minKillVelocity) {
			hitDamagingObject = true;
		} else if (other.gameObject.tag == "Alien") {
			hitDamagingObject = true;
		}

		if (hitDamagingObject && !isDead) {
			HitByDamagingObject (other.transform);
        }
    }

	public void HitByDamagingObject (Transform damagingObject) {
		if (!leaking) {
			leaking = true;

			// start leak effect
			leakDirection = (damagingObject.position - transform.position).normalized;
			leakEffect.Play ();
			var leakAngle = Mathf.Atan2 (leakDirection.y, leakDirection.x) * Mathf.Rad2Deg;
			leakEffect.transform.rotation = Quaternion.AngleAxis (leakAngle, Vector3.forward);
		} else {
			// hit while already leaking
			RemoveOxygen (hitWhileLeakingDrain);
		}
	}

	void StopLeak () {
		leaking = false;
		leakEffect.Stop ();
	}

	void Die () {
		isDead = true;
		Camera.main.GetComponent<CameraController>().PlayerDied();
		GameController.instance.OnPlayerDeath();
	}
		
	public void EquipWeapon (int weaponIndex) {
		if (weaponMount.childCount > 0) {
			// destory any current weapons
			Destroy (weaponMount.GetChild(0));
		}
		WeaponManager.WeaponData weaponData = WeaponManager.instance.GetDataFromIndex (weaponIndex);
		GameObject newWeapon = Instantiate (weaponData.prefab, weaponMount);
		gun = newWeapon.GetComponent<GunController> ();
	}
}