using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    public static GameController instance;

    UIPanel gameOverUI;
    UIPanel preGameUI;
    UIPanel postGameUI;
    UIPanel gameUI;

    Text failMoneyText;
    Text successMoneyText;
    Text preGameMoneyText;
    Text postGameMoneyText;

    public float moneyAddTime;

    MoneyText moneyText;

    int curSiteMoney;
    int startingMoney;

    void Awake() {
        instance = this;

        SetupUI();

        startingMoney = PlayerPrefs.GetInt("Total_Money", 0);
        preGameMoneyText.text = startingMoney.ToString();

        InitShopItems();
        UnpackAll();
        UnpackEquipedWeapon();
    }

    void SetupUI() {
        gameOverUI = GameObject.Find("GameOverUI").GetComponent<UIPanel>();
        postGameUI = GameObject.Find("PostGameUI").GetComponent<UIPanel>();
        preGameUI = GameObject.Find("PreGameUI").GetComponent<UIPanel>();
        gameUI = GameObject.Find("GameUI").GetComponent<UIPanel>();

        failMoneyText = GameObject.Find("FailMoneyText").GetComponent<Text>();
        successMoneyText = GameObject.Find("SuccessMoneyText").GetComponent<Text>();
        preGameMoneyText = GameObject.Find("PreGameMoneyText").GetComponent<Text>();
        postGameMoneyText = GameObject.Find("PostGameMoneyText").GetComponent<Text>();

        moneyText = FindObjectOfType<MoneyText>();
    }

    public void ShowPregame() {
        preGameUI.ShowAll();
        postGameUI.HideAll();
        gameUI.HideAll();
    }

    public void ShowGameUI() {
        preGameUI.HideAll();
        postGameUI.HideAll();
        gameUI.ShowTop();
    }

    public void HideGameUI() {
        gameUI.HideAll();
    }

    public void ShowPostgame() {
        preGameUI.HideAll();
        postGameUI.ShowAll();
        gameUI.HideAll();
        StartCoroutine(ShowAddMoney(successMoneyText));
        postGameMoneyText.text = (startingMoney + curSiteMoney).ToString();
        //postGameReport.SetTrigger("Open");
        SaveMoney();
    }

    public void OnPlayerDeath() {
        SaveMoney();
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence() {
        HideGameUI();
        yield return new WaitForSeconds(1f); //TODO: link this with the death particle system?
        ShowGameOverUI();
    }

    void ShowGameOverUI() {
        gameOverUI.ShowAll();
        StartCoroutine(ShowAddMoney(failMoneyText));
        //gameOverReport.SetTrigger("Open");
    }

    public void EndGame() {
        SceneManager.LoadScene(0);
    }

    public void CollectMoney(int newMoney) {
        curSiteMoney += newMoney;
        moneyText.UpdateText(curSiteMoney);
    }

    void SaveMoney() {
        PlayerPrefs.SetInt("Total_Money", startingMoney + curSiteMoney);
    }

    IEnumerator ShowAddMoney(Text moneyText) {
        yield return new WaitForSeconds(0.5f);

        float p = 0;
        while (p < 1f) {
            moneyText.text = "+"+Mathf.RoundToInt(Mathf.Lerp(0, curSiteMoney, p)).ToString();
            yield return new WaitForEndOfFrame();
            p += Time.deltaTime / moneyAddTime;
        }

        moneyText.text = "+"+curSiteMoney.ToString();
    }

	// SHOPING
	public void BuyUpgrade (string newUpgrade) {
		int currentLevel = PlayerPrefs.GetInt (newUpgrade);
		PlayerPrefs.SetInt (newUpgrade, currentLevel++);
		UnpackUpgrade (newUpgrade, allShopItems);
	}

	void UnpackAll () {
		// get level info from player prefs and update the dictionary
		Dictionary<string, StopItem> newItems = new Dictionary<string, StopItem>();
		foreach (KeyValuePair<string,StopItem> item in allShopItems) {
			newItems.Add (item.Value.upgradeString, item.Value);
			UnpackUpgrade (item.Value.upgradeString, newItems);
		}
		allShopItems = newItems;
	}

	void UnpackEquipedWeapon () {
		string weapon = PlayerPrefs.GetString ("weapon", "pistol");
		StopItem item = allShopItems[weapon];
		item.equiped = true;
		allShopItems [weapon] = item;
		print ("Unpacking " + item.upgradeString + " as equiped weapon ");
	}

	void UnpackUpgrade (string upgrade, Dictionary<string, StopItem> allItems) {
		StopItem item = allItems [upgrade];
		item.currentLevel = PlayerPrefs.GetInt (upgrade, 0);
		allItems [upgrade] = item;
		print ("Unpacking " + item.upgradeString + " with level " + item.currentLevel);
	}

	void InitShopItems () {
		foreach (var item in shopItems) {
			allShopItems.Add (item.upgradeString, item);
		}
	}

	public Dictionary<string, StopItem> allShopItems = new Dictionary<string, StopItem>();
	// for inspector
	public List<StopItem> shopItems;

	[System.Serializable]
	public struct StopItem {
		// general
		public string upgradeString;
		public int currentLevel;
		public int[] costs;
		public string description;

		// weapon specific
		public bool weapon;
		public bool equiped;

		// upgrade specific
		public string topText;
	}
}
