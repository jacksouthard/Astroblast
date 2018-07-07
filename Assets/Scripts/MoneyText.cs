using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneyText : MonoBehaviour {
    public float showTime;
    public float fadeTime;

    RectTransform rt;
    Text text;
    Image icon;
    float iconWidth;
    float p;

    float nextFadeTime;

	void Awake() {
        rt = GetComponent<RectTransform>();
        text = GetComponentInChildren<Text>();
        icon = GetComponentInChildren<Image>();
        iconWidth = -icon.rectTransform.anchoredPosition.x + icon.rectTransform.sizeDelta.x;

        text.color = Color.clear;
        icon.color = Color.clear;
        p = 1f;
	}

	void LateUpdate() {
        rt.anchoredPosition = new Vector2((text.preferredWidth + iconWidth) / 2, rt.anchoredPosition.y); //center

        if (p < 1f && Time.time > nextFadeTime) {
            text.color = Color.Lerp(text.color, Color.clear, p);
            icon.color = Color.Lerp(text.color, Color.clear, p);
            p += Time.deltaTime / fadeTime;
        }
	}

    public void UpdateText(int money) {
        nextFadeTime = Time.time + showTime;
        text.color = Color.white;
        icon.color = Color.white;
        p = 0;
        text.text = money.ToString();
    }
}
