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
    UIPanel shopUI;
    UIPanel mapUI;

    Text failMoneyText;
    Text successMoneyText;
    Text shopMoneyText;
    Text preGameMoneyText;
    Text postGameMoneyText;
    Text preGameLocationText;
    Text postGameLocationText;

    public float moneyAddTime;

    MoneyText moneyText;

    int curSiteMoney;
    public int totalMoney { get; private set; }

    bool isInPostGame = false;

    void Awake() {
        instance = this;
        SetupUI();
    }

    void Start() {
        totalMoney = PlayerPrefs.GetInt("Total_Money", 0);
//		totalMoney = 10000;
        preGameMoneyText.text = totalMoney.ToString();

        // shop
        InitShopItems();
//		ClearUpgrades ();
		GetInitialItems();

        UnpackAll();
        UnpackEquipedWeapon();
        Shop.instance.Init();

        // map
        UnpackAstroidLocationMap();
        AsteroidMap.instance.Init();

        int curAsteroid = PlayerPrefs.GetInt("Current_Asteroid", 0);
        string curAsteroidName = astroidLocations[curAsteroid].name;
        preGameLocationText.text = curAsteroidName;
        postGameLocationText.text = curAsteroidName;
    }

    void SetupUI() {
        gameOverUI = GameObject.Find("GameOverUI").GetComponent<UIPanel>();
        postGameUI = GameObject.Find("PostGameUI").GetComponent<UIPanel>();
        preGameUI = GameObject.Find("PreGameUI").GetComponent<UIPanel>();
        gameUI = GameObject.Find("GameUI").GetComponent<UIPanel>();
        shopUI = GameObject.Find("ShopUI").GetComponent<UIPanel>();
        mapUI = GameObject.Find("MapUI").GetComponent<UIPanel>();

        failMoneyText = GameObject.Find("FailMoneyText").GetComponent<Text>();
        successMoneyText = GameObject.Find("SuccessMoneyText").GetComponent<Text>();
        preGameMoneyText = GameObject.Find("PreGameMoneyText").GetComponent<Text>();
        postGameMoneyText = GameObject.Find("PostGameMoneyText").GetComponent<Text>();
        shopMoneyText = GameObject.Find("ShopMoneyText").GetComponent<Text>();
        preGameLocationText = GameObject.Find("PreGameLocationText").GetComponent<Text>();
        postGameLocationText = GameObject.Find("PostGameLocationText").GetComponent<Text>();

        moneyText = FindObjectOfType<MoneyText>();
    }

    void GetInitialItems() {
        if (!PlayerPrefs.HasKey("weapon")) {
            BuyUpgrade("pistol");
        }
    }

    public void ShowPregame() {
        preGameUI.ShowAll();
        postGameUI.HideAll();
        gameUI.HideAll();
        shopUI.HideAll();
        mapUI.HideAll();
    }

    public void ShowGameUI() {
        preGameUI.HideAll();
        postGameUI.HideAll();
        gameUI.ShowTop();
        shopUI.HideAll();
        mapUI.HideAll();
    }

    public void HideGameUI() {
        gameUI.HideAll();
    }

    public void ShowPostgame() {
        preGameUI.HideAll();
        postGameUI.ShowAll();
        gameUI.HideAll();
        shopUI.HideAll();
        mapUI.HideAll();

        if (!isInPostGame) {
            isInPostGame = true;
            StartCoroutine(ShowAddMoney(successMoneyText));
            SaveMoney();
        } else {
            successMoneyText.text = "+" + curSiteMoney;
        }

        postGameMoneyText.text = totalMoney.ToString();
    }

    public void ShowShop() {
        Shop.instance.Reset();
        Shop.instance.OnOpen();
        preGameUI.HideAll();
        postGameUI.HideAll();
        gameUI.HideAll();
        shopUI.ShowAll();
        mapUI.HideAll();
        shopMoneyText.text = totalMoney.ToString();
    }

    public void CloseShop() {
        Shop.instance.OnClose();
        if (isInPostGame) {
            ShowPostgame();
        } else {
            ShowPregame();
        }
    }

    public void ShowMap() {
        AsteroidMap.instance.OnOpen();
        preGameUI.HideAll();
        postGameUI.HideAll();
        gameUI.HideAll();
        shopUI.HideAll();
        mapUI.ShowAll();
    }

    public void CloseMap() {
        AsteroidMap.instance.OnClose();
        if (isInPostGame) {
            ShowPostgame();
        } else {
            ShowPregame();
        }
    }

    public void OnPlayerDeath() {
		curSiteMoney = Mathf.RoundToInt ((float)curSiteMoney * PlayerController.instance.recovery);
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
    }

    public void EndGame() {
        SceneManager.LoadScene("Game");
    }

	public void LoadCheatScene () {
		SceneManager.LoadScene("Cheats");
	}

    public void CollectMoney(int newMoney) {
        curSiteMoney += newMoney;
        moneyText.UpdateText(curSiteMoney);
    }

    void SaveMoney() {
        totalMoney += curSiteMoney;
        PlayerPrefs.SetInt("Total_Money", totalMoney);
    }

    IEnumerator ShowAddMoney(Text text) {
        yield return new WaitForSeconds(0.5f);

        float p = 0;
        while (p < 1f) {
            text.text = "+"+Mathf.RoundToInt(Mathf.Lerp(0, curSiteMoney, p)).ToString();
            yield return new WaitForEndOfFrame();
            p += Time.deltaTime / moneyAddTime;
        }

        text.text = "+"+curSiteMoney.ToString();
    }

	// SHOPING
	public void BuyUpgrade (string newUpgrade) {
		int currentLevel = PlayerPrefs.GetInt (newUpgrade);
        if (currentLevel < allShopItems[newUpgrade].costs.Length) {
            totalMoney -= allShopItems[newUpgrade].costs[allShopItems[newUpgrade].currentLevel];
            PlayerPrefs.SetInt("Total_Money", totalMoney);

            PlayerPrefs.SetInt(newUpgrade, ++currentLevel);
            UnpackUpgrade(newUpgrade, allShopItems);

            if (allShopItems[newUpgrade].weapon) {
                EquipItem(newUpgrade);
            }

            shopMoneyText.text = totalMoney.ToString();
        } else if(allShopItems[newUpgrade].weapon) {
            EquipItem(newUpgrade);
        }

        Shop.instance.UpdateUI(newUpgrade);
	}

    void EquipItem(string itemName) {
        string curEquippedWeaponName = WeaponManager.instance.GetEquipedWeapon().name;
        allShopItems[curEquippedWeaponName].equiped = false;
        allShopItems[itemName].equiped = true;
        PlayerPrefs.SetString("weapon", itemName);
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

		// apply upgrade effect
		if (upgrade == "tank") {
			// oxygen tank
			PlayerController.instance.SetTankMultiplier (item.currentLevel, item.costs.Length);
		} else if (upgrade == "reach") {
			// reach
			PlayerController.instance.SetReachMultiplier (item.currentLevel, item.costs.Length);
		} else if (upgrade == "patches") {
			// patches
			PlayerController.instance.SetPatches (item.currentLevel);
		} else if (upgrade == "recovery") {
			// recovery
			PlayerController.instance.SetRecovery (item.currentLevel, item.costs.Length);
		}
	}

	void ClearUpgrades () {
		foreach (KeyValuePair<string,StopItem> item in allShopItems) {
			PlayerPrefs.SetInt (item.Key, 0);
		}
		PlayerPrefs.SetString ("weapon", "pistol");
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
    public class StopItem {
		// general
		public string upgradeString;
		public int currentLevel;
		public int[] costs;
		public string description;
		public bool dynamicText;

		// weapon specific
		public bool weapon;
		public bool equiped;

		// upgrade specific
		public string topText;
	}

	// ASTROIDS
	public int farthestAstroid;
    public int currentAstroid;

	void UnpackAstroidLocationMap () {
		farthestAstroid = PlayerPrefs.GetInt ("farthest", 0);
		print ("Unpacking astroid map with farthest location: " + farthestAstroid);
	}
		
	public void UnlockNewAstroid (int newFarthestAstroid) {
		int curFarthest = PlayerPrefs.GetInt ("farthest", 0);
		if (newFarthestAstroid > curFarthest) {
			print ("Unlocking astroid " + newFarthestAstroid);
			PlayerPrefs.SetInt ("farthest", newFarthestAstroid);
			UnpackAstroidLocationMap ();
			AsteroidMap.instance.Init ();
		}
	}

    public void SelectNewAstroid(int newSelected) {
        int curSelected = PlayerPrefs.GetInt("Current_Asteroid", 0);
        if (newSelected != curSelected) {
            PlayerPrefs.SetInt("Current_Asteroid", newSelected);
            EndGame();
        }
    }

	public List<AstroidLocation> astroidLocations = new List<AstroidLocation>();
	[System.Serializable]
	public struct AstroidLocation {
        public string name;
		public Color color;
        public float xPosition;
	}
}
