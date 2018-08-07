using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour {
	public static MessageManager instance;

	public Message[] tutorialMessages;
	Message curMessage;
	int tutorialIndex = 0;
	bool tutorialOver = true;
	bool popupActive = false;

	float nextDisplayDepth;

	public UIPanel messagePanel;
	public Text bottomText;
	public Text popupText;
	int popupIndex = 0;

	// timer
	public float popupSkipDelay;
	float skipTimer;
	bool skipping = false;

	void Awake () {
		instance = this;
	}

	public void StartTutorial () {
		nextDisplayDepth = tutorialMessages [0].displayDepth;
		tutorialOver = false;
	}

	public void EndTutorial () {
		tutorialOver = true;
		HideBottom ();
	}

	void DisplayMessage (int index) {
//		print ("Displaying " + index);
		curMessage = tutorialMessages [index];
			
		if (curMessage.popupTexts.Length > 0) { // show popup UI with message
			popupIndex = 0;
			ShowPopup ();
		} else if (curMessage.bottomText != "") { // show bottom UI with message
			ShowBottom();
		}

		if (index >= tutorialMessages.Length - 1) {
			print ("All messages displayed");
			tutorialOver = true;
		} else {
			nextDisplayDepth = tutorialMessages [index + 1].displayDepth;
		}
	}

	void ShowPopup () {
//		print ("Popup: " + curMessage.popupText);
		Time.timeScale = 0f;
		popupText.text = curMessage.popupTexts[popupIndex];
		messagePanel.ShowReport ();	

		skipTimer = popupSkipDelay;
		skipping = true;
	}
		
	void HidePopup () {
		popupActive = false;

		if (popupIndex <= curMessage.popupTexts.Length - 2) {
			popupIndex++;
			ShowPopup ();
		} else {
			Time.timeScale = 1f;
			messagePanel.HideReport ();

			if (curMessage.bottomText != "") {
				ShowBottom ();
			}
		}
	}

	void ShowBottom () {
//		print ("Popup: " + curMessage.bottomText);
		bottomText.text = curMessage.bottomText;
		messagePanel.ShowBottom ();		
	}

	void HideBottom () {
		messagePanel.HideBottom ();		
	}

	void Update () {
		if (skipping) {
			skipTimer -= Time.unscaledDeltaTime;
			if (skipTimer <= 0f) {
				skipping = false;
				popupActive = true;
			}
		}

		if (popupActive && Input.GetMouseButtonDown (0)) {
			// for hiding popups
			HidePopup();
		}

		if (!tutorialOver && TerrainManager.instance.farthestY < nextDisplayDepth) {
			// hide previous message
			if (tutorialIndex > 0 && tutorialMessages [tutorialIndex].bottomText != "") {
				messagePanel.HideBottom ();
			}
				
			// show new message
			DisplayMessage (tutorialIndex);
			tutorialIndex++;
		}
	}		

	[System.Serializable]
	public struct Message {
		public float displayDepth;
		public string[] popupTexts;
		public string bottomText;
	}
}
