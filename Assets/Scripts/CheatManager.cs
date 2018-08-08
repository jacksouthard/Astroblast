using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CheatManager : MonoBehaviour {
	public Text curMoneyText;

	void Start () {
		int curMoney = PlayerPrefs.GetInt("Total_Money", 0);
		curMoneyText.text = "Have " + curMoney;	
	}

	public void Reset () {
		PlayerPrefs.DeleteAll ();
		ReturnToGame ();
	}

	public void UnlockLevels () {
		PlayerPrefs.SetInt ("farthest", 1000);
	}

	public void AddMoney () {
		int curMoney = PlayerPrefs.GetInt("Total_Money", 0);
		curMoney += 100;
		PlayerPrefs.SetInt("Total_Money", curMoney);
		curMoneyText.text = "Have " + curMoney;
	}

	public void ReturnToGame () {
		SceneManager.LoadScene ("Game");
	}
}
