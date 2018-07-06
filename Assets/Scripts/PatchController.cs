using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatchController : MonoBehaviour {
    public static PatchController instance;

    PlayerController player;
    Animator anim;

    void Awake() {
        instance = this;
    }

	void Start() {
        anim = GetComponent<Animator>();
        player = FindObjectOfType<PlayerController>();
	}

	public void ShowPatchButton() {
        anim.SetBool("Show", true);
    }

    public void ApplyPatch() {
        anim.SetBool("Show", false);
    }

    void OnApplyPatchComplete() { //called through animation
        player.StopLeak();
    }
}
