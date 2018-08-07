using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIAnimator : MonoBehaviour {
    public UnityEvent OnHideComplete;
//	public UnityEvent OnShowComplete;
    Animator anim;

	void Start() {
        anim = GetComponent<Animator>();
	}

	public void Show() {
        anim.SetBool("Show", true);
    }

    public void Hide() {
        anim.SetBool("Show", false);
    }

    void OnHideCompleted() { //called through animation
        OnHideComplete.Invoke();
    }

//	void OnShowCompleted() { //called through animation
//		OnShowComplete.Invoke();
//	}
}
