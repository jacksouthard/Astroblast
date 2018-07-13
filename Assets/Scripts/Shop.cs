using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour {
    public static Shop instance;

    public GameObject shopItemPrefab;

    //public Text upgradeTitleText;
    //public Text weaponTitleText;

    //public GridLayoutGroup upgradeParent;
    //public GridLayoutGroup weaponParent;

    public RectTransform content;
    //public float padding;

    public Button bottomButton;
    public Text buttonText;
    public Text descriptionText;

    public ScrollRect scrollArea;

    Dictionary<string, UIShopItem> uiShopItems = new Dictionary<string, UIShopItem>();

	void Awake() {
        instance = this;
        scrollArea.enabled = false;
	}

	public void Init() {
        //float currentHeight = 0;

        List<GameController.StopItem> upgradeItems = new List<GameController.StopItem>();
        List<GameController.StopItem> weaponItems = new List<GameController.StopItem>();

        foreach (KeyValuePair<string, GameController.StopItem> pair in GameController.instance.allShopItems) {
            if (pair.Value.weapon) {
                weaponItems.Add(pair.Value);
            } else {
                upgradeItems.Add(pair.Value);
            }
        }

        foreach (GameController.StopItem item in upgradeItems) {
            GameObject newItem = Instantiate(shopItemPrefab, content.transform);
            UIShopItem ui = newItem.GetComponent<UIShopItem>();
            ui.Init(item.upgradeString);

            uiShopItems.Add(item.upgradeString, ui);
        }

        foreach (GameController.StopItem item in weaponItems) {
            GameObject newItem = Instantiate(shopItemPrefab, content.transform);
            UIShopItem ui = newItem.GetComponent<UIShopItem>();
            ui.Init(item.upgradeString);

            uiShopItems.Add(item.upgradeString, ui);
        }

        // FOR SEPARATE GRIDS
        //upgradeTitleText.rectTransform.anchoredPosition = new Vector2(0, -currentHeight);
        //currentHeight += upgradeTitleText.rectTransform.rect.height + padding;

        //foreach (GameController.StopItem item in upgradeItems) {
        //    GameObject newItem = Instantiate(shopItemPrefab, upgradeParent.transform);
        //    UIShopItem ui = newItem.GetComponent<UIShopItem>();
        //    ui.Init(item.upgradeString);

        //    uiShopItems.Add(item.upgradeString, ui);
        //}

        //RectTransform upgradeRT = upgradeParent.GetComponent<RectTransform>();
        //upgradeRT.anchoredPosition = new Vector2(0, -currentHeight);
        //int numUpgradeRows = Mathf.CeilToInt(upgradeItems.Count / (upgradeRT.rect.width / (upgradeParent.cellSize.x + upgradeParent.spacing.x)));
        //currentHeight += Mathf.Abs(numUpgradeRows * upgradeParent.cellSize.y + padding);
        //if (Mathf.Abs(numUpgradeRows) > 1) {
        //    currentHeight += upgradeParent.spacing.y * (numUpgradeRows - 1);
        //}

        //weaponTitleText.rectTransform.anchoredPosition = new Vector2(0, -currentHeight);
        //currentHeight += weaponTitleText.rectTransform.rect.height + padding;

        //foreach (GameController.StopItem item in weaponItems) {
        //    GameObject newItem = Instantiate(shopItemPrefab, weaponParent.transform);
        //    UIShopItem ui = newItem.GetComponent<UIShopItem>();
        //    ui.Init(item.upgradeString);

        //    if (item.equiped) {
        //        newItem.GetComponent<UIShopItem>().Equip();
        //    }

        //    uiShopItems.Add(item.upgradeString, ui);
        //}

        //RectTransform weaponRT = weaponParent.GetComponent<RectTransform>();
        //weaponRT.anchoredPosition = new Vector2(0, -currentHeight);
        //int numWeaponRows = Mathf.CeilToInt(upgradeItems.Count / (weaponRT.rect.width / (weaponParent.cellSize.x + weaponParent.spacing.x)));
        //currentHeight += Mathf.Abs(numWeaponRows * weaponParent.cellSize.y + padding);
        //if (Mathf.Abs(numWeaponRows) > 1) {
        //    currentHeight += weaponParent.spacing.y * (numWeaponRows - 1);
        //}

        //content.sizeDelta = new Vector2(0, currentHeight);

        Reset();
    }

    public void OnOpen() {
        StartCoroutine(OpenSequence());
    }

    IEnumerator OpenSequence() {
        yield return new WaitForSeconds(0.5f);
        scrollArea.enabled = true;
        content.anchoredPosition = Vector2.zero;
    }

    public void OnClose() {
        scrollArea.enabled = false;
    }

	public void Reset() {
        buttonText.text = "Return to site";
        bottomButton.interactable = true;
        descriptionText.text = "";
        if (UIShopItem.selectedItem != null) {
            UIShopItem.selectedItem.Deselect();
        }

        UIShopItem.selectedItem = null;
	}

	public void OnItemSelected(string itemKey) {
        GameController.StopItem item = GameController.instance.allShopItems[itemKey];

        if (item.equiped) {
            buttonText.text = "EQUIPPED";
            bottomButton.interactable = false;
        } else if (item.currentLevel >= item.costs.Length) {
            if (item.weapon) {
                buttonText.text = "Equip";
                bottomButton.interactable = true;
            } else {
                buttonText.text = "MAXED OUT";
                bottomButton.interactable = false;
            }
        } else {
            buttonText.text = "Buy for " + item.costs[item.currentLevel];
            bottomButton.interactable = (item.costs[item.currentLevel] <= GameController.instance.totalMoney);
        }

        descriptionText.text = item.description;
    }

    public void OnBottomButtonPressed() {
        if (UIShopItem.selectedItem == null) {
            GameController.instance.CloseShop();
        } else {
            GameController.instance.BuyUpgrade(UIShopItem.selectedItem.key);
        }
    }

    public void UpdateUI(string purchasedItem) {
        uiShopItems[purchasedItem].Refresh();
    }
}
