using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIShopItem : MonoBehaviour {
    public Text mainText, subTitleText;
    public Image weaponIcon;
    public GameObject sideBorders;
    public Image[] borders;
    public List<Image> bars;
    public bool isSelected;

    public Color orangeColor;
    public Color grayColor;

    public void Init(string itemKey) {
        GameController.StopItem data = GameController.instance.allShopItems[itemKey];

        subTitleText.text = data.upgradeString;
        if (!data.weapon) {
            mainText.text = data.topText;
            weaponIcon.enabled = false;
        } else {
            mainText.enabled = false;
            weaponIcon.sprite = WeaponManager.instance.GetIconFromName(itemKey);
        }

        int numLevels = data.costs.Length;
        for (int i = 0; i < numLevels; i++) {
            if (data.currentLevel > i) {
                
            }

            if (i > 0) {
                GameObject newBarObj = Instantiate(bars[0].gameObject, bars[0].transform.parent);
                bars.Add(newBarObj.GetComponent<Image>());
            }
        }
    }
}
