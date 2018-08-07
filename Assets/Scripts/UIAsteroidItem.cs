using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAsteroidItem : MonoBehaviour {
    int id;

    public void Init(int asteroidId, Image line) {
        id = asteroidId;

        Button button = GetComponentInChildren<Button>();
        Text text = GetComponentInChildren<Text>();

        GameController.AstroidLocation location = GameController.instance.astroidLocations[id];
        text.text = location.name;

        bool isUnlocked = GameController.instance.farthestAstroid >= asteroidId;
        float alpha = (isUnlocked) ? 1f : 0.25f;
        button.image.color = new Color(location.color.r * alpha, location.color.g * alpha, location.color.b * alpha, 1);
        text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);

        if (line != null) {
            line.color = new Color(line.color.r, line.color.g, line.color.b, alpha);
        }

        button.interactable = isUnlocked;
	}

    public void OnButtonPressed() {
        GameController.instance.SelectNewAstroid(id);
    }
}
