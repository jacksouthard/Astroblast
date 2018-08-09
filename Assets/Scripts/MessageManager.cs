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

	public GameObject popupBlocker;
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

	void Start () {
		
	}

	public void StartTutorial () {
		nextDisplayDepth = tutorialMessages [0].displayDepth;
		tutorialOver = false;
	}

	public void EndTutorial () {
		tutorialOver = true;
		HideBottom ();
	}

	void DisplayNextTutorialMessage (int index) {
		DisplayMessage (tutorialMessages [index]);
		if (index >= tutorialMessages.Length - 1) {
			print ("All messages displayed");
			tutorialOver = true;
		} else {
			nextDisplayDepth = tutorialMessages [index + 1].displayDepth;
		}
	}

	void DisplayMessage (Message displayMessage) {
		curMessage = displayMessage;

		if (curMessage.popupTexts.Length > 0) { // show popup UI with message
			popupIndex = 0;
			ShowPopup ();
		} else if (curMessage.bottomText != "") { // show bottom UI with message
			ShowBottom();
		}
	}

	void ShowPopup () {
//		print ("Popup: " + curMessage.popupText);
		Time.timeScale = 0f;
		popupBlocker.SetActive (true);
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
			popupBlocker.SetActive (false);
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

	public void HideBottom () {
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
			DisplayNextTutorialMessage (tutorialIndex);
			tutorialIndex++;
		}
	}

	// SITUATIONAL MESSAGES
	[Header("Situational")]
	public Message leak;
	public Message openShop;
	public Message openMap;
//	public Message enterPostGame;
	public Message collectTreasure;
	//	public Message lowO2;

	public void OnLeak () {
		int curTimesDisplayed = PlayerPrefs.GetInt ("Mleak", 0);
		if (curTimesDisplayed == 0) {
			DisplayMessage (leak);
			PlayerPrefs.SetInt ("Mleak", curTimesDisplayed + 1);		
		}
	}
		
	public void OnShopOpened () {
		int curTimesDisplayed = PlayerPrefs.GetInt ("Mshop", 0);
		if (curTimesDisplayed == 0) {
			DisplayMessage (openShop);
			PlayerPrefs.SetInt ("Mshop", curTimesDisplayed + 1);		
		}	
	}

	public void OnMapOpened () {
		int curTimesDisplayed = PlayerPrefs.GetInt ("Mmap", 0);
		if (curTimesDisplayed == 0) {
			DisplayMessage (openMap);
			PlayerPrefs.SetInt ("Mmap", curTimesDisplayed + 1);		
		}
	}

//	public void OnEnterPostGame () {
//		DisplayMessage (enterPostGame);
//	}

	public void OnCollectTreasure () {
		if (TerrainManager.instance.curAstroidIndex == 0) {
			DisplayMessage (collectTreasure);
		}
	}




	[System.Serializable]
	public struct Message {
		public float displayDepth;
		public string[] popupTexts;
		public string bottomText;
	}
}
