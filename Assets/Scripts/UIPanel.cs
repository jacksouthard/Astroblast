using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPanel : MonoBehaviour {
    UIAnimator topAnim;
    UIAnimator bottomAnim;

    void Awake() {
        topAnim = transform.Find("TopAnchor").GetComponent<UIAnimator>();
        bottomAnim = transform.Find("BottomAnchor").GetComponent<UIAnimator>();
    }

    public void ShowTop() {
        topAnim.Show();
    }

    public void HideTop() {
        topAnim.Hide();
    }

    public void ShowBottom() {
        bottomAnim.Show();
    }

    public void HideBottom() {
        bottomAnim.Hide();
    }

    public void ShowAll() {
        ShowTop();
        ShowBottom();
    }

    public void HideAll() {
        HideTop();
        HideBottom();
    }
}
