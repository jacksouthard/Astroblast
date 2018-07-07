using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAlien : MonoBehaviour {
	protected enum State {
		idle,
		awakening,
		awakened,
		attacking,
		dead
	};
	protected State state = State.idle;
	protected bool active;

	public int health;

	protected Transform target;
	protected Animator anim;
	protected Rigidbody2D rb;

    public bool canWalk;
    public float walkSpeed;
    public float rotSpeed;
    protected int curIndex;
    int nextIndex { get { return curAstroid.GetAdjacentVertIndex(curIndex, dir); } }
    Quaternion goalRot;
    Quaternion rotOffset; //if the alien is moving backwards, angles must be flipped
    float curEdgeDist;
    float curRatio;
    int dir;

	// hit and leaking
	Transform leakEffect;
	ParticleSystem leakEffectParticles;
	bool leaking = false;
	Vector2 leakDirection;
	float leakForce = 4000f;
	float leakTimeOnHit = 0.3f;
	float leakTimeAfterDeath = 4f;
	float leakTimer = 0;

	protected AstroidSpawner curAstroid;

	void Awake () {
		anim = GetComponent<Animator> ();
		rb = GetComponent<Rigidbody2D> ();
		leakEffect = transform.Find ("LeakEffect");
		leakEffectParticles = leakEffect.GetComponent<ParticleSystem> ();
	}

	void Start () {
		Initiated ();
        Activate();
	}

    public void InitPos(AstroidSpawner astroid, int startingIndex) {
		curAstroid = astroid;
		curIndex = startingIndex;

		if (!canWalk) {
            return;
        }

        dir = (Random.Range(0, 2) * 2) - 1; //either -1 or 1

        if (dir == 1) {
            rotOffset = Quaternion.Euler(0, 0, 0);
        } else {
            curIndex = curAstroid.GetAdjacentVertIndex(startingIndex, 1);
            rotOffset = Quaternion.Euler(0, 0, 180);
        }

        curRatio = 0.5f; //aliens start at the midpoint between vertices
        curEdgeDist = curAstroid.GetDistBetweenVerts(curIndex, nextIndex);
    }

	protected virtual void Initiated () {}

	void Update() {
		if (leaking) {
			rb.AddForce (-leakDirection * leakForce * Time.deltaTime);
			leakTimer -= Time.deltaTime;
			if (leakTimer <= 0f) {
				StopLeaking ();
			}
		}

        AlienUpdate();
	}

    protected virtual void AlienUpdate() {
        if (canWalk && state == State.idle) {
            Walk();
        }
    }

    void Walk() {
        curRatio += Time.deltaTime / curEdgeDist * walkSpeed;
        if (curRatio >= 1) {
            curRatio -= 1;
            SwitchSides();
        }
        if (curRatio >= 0.8f) {
            SwitchAngles();
        }

        transform.position = curAstroid.GetPosBetweenVerts(curIndex, nextIndex, curRatio).point;
        transform.rotation = Quaternion.Lerp(transform.rotation, goalRot, Time.deltaTime * rotSpeed);
    }

    void SwitchSides() {
        curIndex = nextIndex;
        curEdgeDist = curAstroid.GetDistBetweenVerts(curIndex, nextIndex);
    }

    void SwitchAngles() {
        goalRot = curAstroid.GetPosBetweenVerts(nextIndex, curAstroid.GetAdjacentVertIndex(nextIndex, dir), curRatio).rotation * rotOffset; //when quaternions multiply, euler angles add
    }

	void OnTriggerEnter2D (Collider2D coll) {
		if (coll.gameObject.tag == "Player") {
			if (state != State.dead) {
				if (state == State.idle) {
					target = coll.transform;
					Awaken ();
				} else if (state == State.awakened) {
					TripAttack (); // for shooting enemies
				}
			}
		} else if (coll.tag == "ActiveZone") {
			if (!active) {
				Activate ();
			}
		}
	}

	void OnTriggerExit2D (Collider2D coll) {
		if (coll.tag == "ActiveZone") {
			if (active) {
				Deactivate ();
			}
		}
	}

	protected virtual void Activate () {
		active = true;
        if (canWalk) {
            anim.SetBool("IsActive", true);
        }
	}

	protected virtual void Deactivate () {
		active = false;
        if (canWalk) {
            anim.SetBool("IsActive", false);
        }
		rb.velocity = Vector2.zero;

		if (state == State.dead) {
			Destroy (gameObject);
		}
	}

	protected virtual void TripAttack () {}

	protected virtual void Awaken () {
		anim.SetTrigger ("Awaken");
		state = State.awakening;
	}

	void OnCollisionEnter2D (Collision2D coll) {
		if (coll.gameObject.tag == "Bullet") {
			Bullet bullet = coll.gameObject.GetComponent<Bullet> ();
			bullet.Hit ();

			if (state != State.dead) {
				TakeDamage (bullet.damage, coll.transform);

				if (state == State.idle) {
					target = GameObject.Find ("Player").transform;
					Awaken ();
				}
			}
		}
	}

	void TakeDamage (int damage, Transform source) {
		health -= damage;
		if (health <= 0) {
			StartLeaking (leakTimeAfterDeath, source);
			Die ();
		} else {
			StartLeaking (leakTimeOnHit, source);
		}
	}

	void StartLeaking (float time, Transform source) {
		leaking = true;
		leakTimer = time;

		// setup effect
		leakDirection = (source.position - transform.position).normalized;
		leakEffectParticles.Play ();
		var leakAngle = Mathf.Atan2 (leakDirection.y, leakDirection.x) * Mathf.Rad2Deg;
		leakEffect.rotation = Quaternion.AngleAxis (leakAngle, Vector3.forward);
	}

	void StopLeaking () {
		leaking = false;
		leakEffectParticles.Stop ();
	}

	void Die () {
		state = State.dead;
		rb.isKinematic = false;
		anim.SetTrigger ("Die");

		PlayerDamaging possibleDamaging = GetComponent<PlayerDamaging> ();
		if (possibleDamaging != null) {
			Destroy (possibleDamaging);
		}
	}
}
