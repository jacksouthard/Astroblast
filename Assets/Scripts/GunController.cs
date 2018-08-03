using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {
	PlayerController pc;
	Transform bulletSpawn;
    GameObject muzzleFlash;

	public GameObject projectile;
	public int shots = 1;
	public int damage;
	float forceToDamageRatio = 70f; // multiplied by bullet damage to determain force
	public float accuracy;
	public float speed;
	public float lifetime;
	public float knockback;
	public float fireRate;
	float timeUntilFire = 0f;
	bool canFire = true;

    Rigidbody2D rb;

	// Use this for initialization
	void Start () {
		pc = GetComponentInParent<PlayerController> ();
		bulletSpawn = transform.Find ("BulletSpawn");
        muzzleFlash = transform.Find ("MuzzleFlash").gameObject;

        rb = GetComponentInParent<Rigidbody2D>();

        muzzleFlash.SetActive(false);
	}

	public void TryFire () {
		if (canFire) {
			timeUntilFire = fireRate;
			canFire = false;

			Fire ();
		}
	}

	void Fire () {
		float angleIncriment = 0;
		if (shots > 0) {
			angleIncriment = accuracy * 2f / (float)(shots - 1);
		}

		for (int i = 0; i < shots; i++) {
			GameObject newProjectile = Instantiate (projectile, bulletSpawn.position, bulletSpawn.rotation);
			Bullet newBullet = newProjectile.GetComponent<Bullet> ();
			newBullet.damage = damage;
			newBullet.force = damage * forceToDamageRatio;
			Rigidbody2D projectileRb = newProjectile.GetComponent<Rigidbody2D> ();
			float rotOffset;
			if (shots == 1) {
				rotOffset = Random.Range (-accuracy, accuracy);
			} else {
				rotOffset = -accuracy + (angleIncriment * i);
			}
			projectileRb.velocity = rb.velocity;
			newProjectile.transform.rotation = Quaternion.Euler (0f, 0f, newProjectile.transform.rotation.eulerAngles.z + rotOffset); 
			print (newProjectile.transform.eulerAngles.z);
			projectileRb.AddForce (newProjectile.transform.right * speed);
			Destroy (newProjectile, lifetime);		
		}

		pc.ProjectileFire (knockback);

        StartCoroutine(ShowMuzzleFlash());
	}
	
	void Update () {
		if (!canFire) {
			timeUntilFire -= Time.deltaTime;
			if (timeUntilFire <= 0f) {
				canFire = true;
			}
		}
	}

    IEnumerator ShowMuzzleFlash() {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.SetActive(false);
    }
}
