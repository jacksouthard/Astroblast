using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAlien : MonoBehaviour {
	protected enum State {
		idle,
		awakening,
		awakened,
		attacking
	};
	protected State state = State.idle;
	protected bool active = true;

	public int health;

	protected Transform target;
	protected Animator anim;
	protected Rigidbody2D rb;

    public bool canWalk;
    public float walkSpeed;
    public float rotSpeed;
    int curIndex;
    int nextIndex { get { return curAstroid.GetAdjacentVertIndex(curIndex, dir); } }
    Quaternion goalRot;
    Quaternion rotOffset; //if the alien is moving backwards, angles must be flipped
    float curEdgeDist;
    float curRatio;
    int dir;

    AstroidSpawner curAstroid;

	void Awake () {
		anim = GetComponent<Animator> ();
		rb = GetComponent<Rigidbody2D> ();
	}

	void Start () {
		Initiated ();
	}

    public void InitPos(AstroidSpawner astroid, int startingIndex) {
        if (!canWalk) {
            return;
        }

        dir = (Random.Range(0, 2) * 2) - 1; //either -1 or 1

        curAstroid = astroid;
        if (dir == 1) {
            curIndex = startingIndex;
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
			if (state == State.idle) {
				target = coll.transform;
				Awaken ();
			} else if (state == State.awakened) {
				TripAttack (); // for shooting enemies
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
	}

	protected virtual void Deactivate () {
		active = false;
		rb.velocity = Vector2.zero;
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
			TakeDamage (bullet.damage);
		}
	}

	void TakeDamage (int damage) {
		health -= damage;
		if (health <= 0) {
			Die ();
		}
	}

	void Die () {
		Destroy (gameObject);
	}
}
