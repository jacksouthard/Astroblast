using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneyText : MonoBehaviour {
    public float showTime;

    RectTransform rt;
    Text text;
    Image icon;
    float iconWidth;

    float nextFadeTime;

	void Awake() {
        rt = GetComponent<RectTransform>();
        text = GetComponentInChildren<Text>();
        icon = GetComponentInChildren<Image>();
        iconWidth = -icon.rectTransform.anchoredPosition.x + icon.rectTransform.sizeDelta.x;
	}

	void LateUpdate() {
        rt.anchoredPosition = new Vector2((text.preferredWidth + iconWidth) / 2, rt.anchoredPosition.y); //center

        if (Time.time > nextFadeTime) {
            text.color = Color.Lerp(text.color, Color.clear, Time.deltaTime);
            icon.color = Color.Lerp(text.color, Color.clear, Time.deltaTime);
        }
	}

    public void UpdateText(int newMoney) {
        nextFadeTime = Time.time + showTime;
        text.color = Color.white;
        icon.color = Color.white;
    }
}
