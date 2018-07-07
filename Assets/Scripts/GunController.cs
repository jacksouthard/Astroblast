using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {
	PlayerController pc;
	Transform bulletSpawn;
    GameObject muzzleFlash;

	public GameObject projectile;
	public int damage;
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
		GameObject newProjectile = Instantiate (projectile, bulletSpawn.position, bulletSpawn.rotation);
		newProjectile.GetComponent<Bullet> ().damage = damage;
		Rigidbody2D projectileRb = newProjectile.GetComponent<Rigidbody2D> ();
		newProjectile.transform.rotation = Quaternion.Euler (0f, 0f, newProjectile.transform.rotation.eulerAngles.z + Random.Range (-accuracy, accuracy)); 
        projectileRb.velocity = rb.velocity;
        projectileRb.AddForce (newProjectile.transform.right * speed);
		Destroy (newProjectile, lifetime);

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
