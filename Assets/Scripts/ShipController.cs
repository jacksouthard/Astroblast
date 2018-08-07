using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour {
    public static ShipController instance;

	bool entering = true;
    public bool skipEntry;
    public bool canExit;
    Animator anim;
    Transform player;
    CameraController camCon;

    void Awake() {
        instance = this;
        camCon = Camera.main.GetComponent<CameraController>();
    }

    void Start() {
        camCon.SetZoom(22f);

        anim = GetComponent<Animator>();

        if (skipEntry) {
            anim.SetTrigger("Hover");
            EnterHover();
        } else {
            anim.SetTrigger("Enter");
            //          camCon.StartTracking (transform);
        }
    }

    void Update() {
		if (entering && Input.GetMouseButtonDown(0)) {
			anim.speed = 10000f; // to skip animation (hacky?)
		}
    }

    void EnterHover() { // called partially as an event in the enter animation
		anim.speed = 1f;
		canExit = true;
		entering = false;
        GameController.instance.ShowPregame();
    }

    void ExitHover() {
        canExit = false;
    }

    void ExitSite() {
        GameController.instance.ShowPostgame();
    }

    void OnCollisionEnter2D(Collision2D coll) {
        if (coll.gameObject.tag == "Player") {
            if (TerrainManager.instance.direction == 1) {
				if (BottomController.instance.hasTreasure) {
					BottomController.instance.TreasureExtracted ();
				}
                PlayerEnter(player);
                GameController.instance.HideGameUI();
				if (TerrainManager.instance.curAstroidIndex == 0) {
					MessageManager.instance.EndTutorial ();
				}
                //              camCon.StartTracking (transform);
                camCon.StartZoom(22f, 2f);
                anim.SetTrigger("Exit");
            }
        }
    }

    // player relations
    public void PlayerEnter(Transform _player) {
        player = _player;
        camCon.StopTracking();
        player.gameObject.SetActive(false);
    }

    public void PlayerExit() {
        if (!canExit) {
            return;
        }

        player.GetComponent<PlayerController>().EquipWeapon();
        GameController.instance.ShowGameUI();
		if (TerrainManager.instance.curAstroidIndex == 0) {
			MessageManager.instance.StartTutorial ();
		}
        player.gameObject.SetActive(true);
        player.position = transform.Find("PlayerSpawn").position;
        camCon.StartTracking(player);
        camCon.StartZoom(16f, 2f);
        canExit = false;
    }
}
