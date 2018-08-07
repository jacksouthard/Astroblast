using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPanel : MonoBehaviour {
    UIAnimator topAnim;
    UIAnimator bottomAnim;
    UIAnimator reportWindow;

    void Awake() {
        topAnim = transform.Find("TopAnchor").GetComponent<UIAnimator>();
        bottomAnim = transform.Find("BottomAnchor").GetComponent<UIAnimator>();

        Transform reportWindowTransform = transform.Find("ReportWindow");
        if (reportWindowTransform != null) {
            reportWindow = reportWindowTransform.GetComponent<UIAnimator>();
        }
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

	public void ShowReport () {
		reportWindow.Show();
	}

	public void HideReport () {
		reportWindow.Hide ();
	}

    public void ShowAll() {
        ShowTop();
        ShowBottom();

        if (reportWindow != null) {
			ShowReport ();
        }
    }

    public void HideAll() {
        HideTop();
        HideBottom();

        if (reportWindow != null) {
			HideReport ();
        }
    }
}
