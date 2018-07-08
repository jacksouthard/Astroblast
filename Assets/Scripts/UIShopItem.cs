using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIShopItem : MonoBehaviour {
    public static UIShopItem selectedItem;
    public static UIShopItem equippedItem;
    bool isSelected { get { return selectedItem == this; } }
    bool isEquipped { get { return equippedItem == this; } }

    public Text mainText, subTitleText;
    public Image weaponIcon;
    public GameObject borderParent;
    Image[] borders;
    public List<Image> bars;

    public Color orangeColor;
    public Color grayColor;

    public string key { get; private set; }

	public void Init(string itemKey) {
        key = itemKey;

        GameController.StopItem data = GameController.instance.allShopItems[key];

        subTitleText.text = data.upgradeString;
        if (!data.weapon) {
            mainText.text = data.topText;
            weaponIcon.enabled = false;
        } else {
            mainText.enabled = false;
            weaponIcon.sprite = WeaponManager.instance.GetIconFromName(key);
        }

        int numLevels = data.costs.Length;
        for (int i = 0; i < numLevels; i++) {
            if (i > 0) {
                GameObject newBarObj = Instantiate(bars[0].gameObject, bars[0].transform.parent);
                bars.Add(newBarObj.GetComponent<Image>());
            }

            bars[i].color = (data.currentLevel > i) ? orangeColor : grayColor;
        }

        borders = borderParent.GetComponentsInChildren<Image>();
        borderParent.SetActive(false);
    }

    public void Refresh() {
        GameController.StopItem data = GameController.instance.allShopItems[key];

        for (int i = 0; i < bars.Count; i++) {
            bars[i].color = (data.currentLevel > i) ? orangeColor : grayColor;
        }

        if (data.equiped) {
            Equip();
        }

        Shop.instance.OnItemSelected(key);
    }

    public void OnButtonPressed() {
        if (!isSelected) {
            Select();
        } else {
            Shop.instance.Reset();
        }
    }

    public void Select() {
        if (selectedItem != null) {
            selectedItem.Deselect();
        }

        borderParent.SetActive(true);

        if (!isEquipped) {
            SetAllBorders(Color.white);
        } else {
            SetAllBorders(orangeColor);
        }

        Shop.instance.OnItemSelected(key);
        selectedItem = this;
    }

    public void Deselect() {
        if (!isEquipped) {
            borderParent.SetActive(false);
        }
    }

    public void Equip() {
        if (equippedItem != null) {
            equippedItem.Unequip();
        }

        borderParent.SetActive(true);
        SetAllBorders(orangeColor);

        equippedItem = this;
    }

    public void Unequip() {
        SetAllBorders(Color.white);
        borderParent.SetActive(false);
    }

    void SetAllBorders(Color newColor) {
        foreach (Image border in borders) {
            border.color = newColor;
        }
    }
}
