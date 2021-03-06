﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
	public static PlayerController instance;

	public float weaponDistance;
    Transform weaponMount;
    GunController gun;

    Rigidbody2D rb;
    float weaponMoveDstToBodyForce = 0.8f;

    public float minKillVelocity;
    public Transform[] arms;

    public float upperMargin = 2f;
    float flipHeightThreshold;
    Transform cam;

    Vector2 lastWeaponAngle = Vector2.right;

    // death
    [Header("Oxygen")]
    float oxygen = 100f;
    public float baseDrain;
    public float leakDrain;
    public float flipThreshold;
    bool flipQued = false;
    public float hitWhileLeakingDrain;
    public Text oxygenText;
	UIFlicker oxygenFlicker;
    public UIAnimator patchAnim;
	public Text patchText;

	[Header("Upgrades")]
	// patches
	public int minPatches;
	[HideInInspector]
	public int patches;
	// tank
	public float minTankMultiplier;
	[HideInInspector]
	public float tankMultiplier = 1f;
	// reach
	public float maxReachMultiplier;
	[HideInInspector]
	public float reachMultiplier = 1f;
	// recovery
	public float minRecovery;
	public float maxRecovery;
	[HideInInspector]
	public float recovery;


    // leaks
    bool leaking = false;
    LightFlicker warningLight;
    ParticleSystem leakEffect;
    Vector2 leakDirection;
    public float leakForce;

    // death
    bool isDead = false;

	void Awake () {
		instance = this;
	}

    void Start() {
        warningLight = GetComponentInChildren<LightFlicker>();
		oxygenFlicker = oxygenText.GetComponent<UIFlicker> ();
        leakEffect = transform.Find("LeakEffect").GetComponent<ParticleSystem>();
        rb = GetComponent<Rigidbody2D>();
        weaponMount = transform.Find("WeaponMount");
        gun = GetComponentInChildren<GunController>();

        cam = Camera.main.transform;
        flipHeightThreshold = FindObjectOfType<CameraController>().baseZoom - upperMargin;

        EquipWeapon();

        // enter ship
        ShipController.instance.PlayerEnter(transform);
    }

    void Update() {
		if (!isDead && Time.timeScale != 0) {
            // leaking
            float amountToRemove = baseDrain;
            if (leaking) {
                rb.AddRelativeForce(-leakDirection * leakForce * Time.deltaTime);
                amountToRemove += leakDrain;
            }
			RemoveOxygen(amountToRemove * Time.deltaTime);

            // shooting
            if (Input.GetMouseButton(0)) {
                Vector2 angleVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                PositionWeapon(angleVector.normalized);
                gun.TryFire();
            }
          
            if (shouldFlip) {
                TerrainManager.instance.SwitchDirections();
            }
        }
    }

    bool shouldFlip {
        get {
            return (TerrainManager.instance.direction == -1 && transform.position.y - cam.position.y > flipHeightThreshold) ||
                    (TerrainManager.instance.direction == 1 && transform.position.y - cam.position.y < -flipHeightThreshold);
        }
    }

	// UPGRADES
	public void SetTankMultiplier (int level, int maxLevel) {
		float levelRatio = (float)level / (float)maxLevel;
		tankMultiplier = Mathf.Lerp (1f, minTankMultiplier, levelRatio);
//		print (level + " / " + maxLevel + " -> " + tankMultiplier);
	}

	public void SetReachMultiplier (int level, int maxLevel) {
		float levelRatio = (float)level / (float)maxLevel;
		reachMultiplier = Mathf.Lerp (1f, maxReachMultiplier, levelRatio);
//		print (level + " / " + maxLevel + " -> " + reachMultiplier);
	}

	public void SetPatches (int level) {
		patches = minPatches + level;
	}

	public void SetRecovery (int level, int maxLevel) {
		float levelRatio = (float)level / (float)maxLevel;
		recovery = Mathf.Lerp (minRecovery, maxRecovery, levelRatio);
//		print (level + " / " + maxLevel + " -> " + recovery);
	}

	void RemoveOxygen (float value) {
		oxygen -= value * tankMultiplier;
//		if (oxygen < flipThreshold) {
//			flipQued = true;
//		}
		oxygen = Mathf.Clamp (oxygen, 0f, 100f);
		oxygenText.text = "" + (int)oxygen + "%";

		if (oxygen <= 0f) {
			Die();
		}
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
			// arm 0 is left arm and 1 is right arm
			arms [0].transform.position = new Vector3 (arms [0].transform.position.x, arms [0].transform.position.y, -1.1f);
			arms [1].transform.position = new Vector3 (arms [1].transform.position.x, arms [1].transform.position.y, -0.9f);
		} else {
			// weapon on left side
			newScale = new Vector3 (1f, -1f, 1f);
			// arm 0 is left arm and 1 is right arm
			arms [0].transform.position = new Vector3 (arms [0].transform.position.x, arms [0].transform.position.y, -0.9f);
			arms [1].transform.position = new Vector3 (arms [1].transform.position.x, arms [1].transform.position.y, -1.1f);
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
		if (isDead) {
			return;
		}

		bool hitDamagingObject = false;
		PlayerDamaging possibleDamage = other.collider.GetComponentInParent<PlayerDamaging> ();
		if (possibleDamage != null) {
			if (possibleDamage.lethal) {
				Die ();
				return;
			}
			if (possibleDamage.minVelocity == 0) {
				hitDamagingObject = true;
			} else if (other.relativeVelocity.magnitude > possibleDamage.minVelocity) {
				hitDamagingObject = true;
			}
		}

		if (hitDamagingObject) {
			HitByDamagingObject (other.transform);
        }
    }

	public void HitByDamagingObject (Transform damagingObject) {
		if (!leaking && !isDead) {
			leaking = true;

			// start leak effect
			warningLight.StartFlicker();
			oxygenFlicker.StartFlicker ();
			leakDirection = (damagingObject.position - transform.position).normalized;
			leakEffect.Play ();
			var leakAngle = Mathf.Atan2 (leakDirection.y, leakDirection.x) * Mathf.Rad2Deg;
			leakEffect.transform.rotation = Quaternion.AngleAxis (leakAngle, Vector3.forward);

			if (patches > 0) {
				patchText.text = "Patch Breach (" + patches + ")";
			} else {
				patchText.text = "No more patches";
				patchText.transform.parent.GetComponent<Button> ().enabled = false;
			}

            patchAnim.Show();

			MessageManager.instance.OnLeak ();
		} else {
			// hit while already leaking
			RemoveOxygen (hitWhileLeakingDrain);
		}
	}

	public void StopLeak () {
		leaking = false;
		patches--;
		warningLight.StopFlicker ();
		oxygenFlicker.StopFlicker ();
		leakEffect.Stop ();

		if (flipQued) {
			flipQued = false;
			TerrainManager.instance.SwitchDirections ();
		}
	}

	void Die () {
		isDead = true;
		Camera.main.GetComponent<CameraController>().PlayerDied();
		GameController.instance.OnPlayerDeath();
		oxygenFlicker.StopFlicker ();

		if (leaking) {
			StopLeak ();
		}
	}
		
	public void EquipWeapon () {
		if (weaponMount.childCount > 0) {
			// destory any current weapons
            Destroy (weaponMount.GetChild(0).gameObject);
		}
		WeaponManager.WeaponData weaponData = WeaponManager.instance.GetEquipedWeapon ();
		GameObject newWeapon = Instantiate (weaponData.prefab, weaponMount);
		newWeapon.transform.localPosition = Vector3.zero;
		gun = newWeapon.GetComponent<GunController> ();
	}

	public void CollectedTreasure (Color treasureColor) {
		Light l = transform.Find ("TreasureLight").GetComponent<Light> ();
		l.enabled = true;
		l.color = treasureColor;
	}
}
