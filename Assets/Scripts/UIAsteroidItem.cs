using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAsteroidItem : MonoBehaviour {
    Image image;
    Text text;

	void Awake() {
        image = GetComponentInChildren<Image>();
        text = GetComponentInChildren<Text>();
	}

    public void Init(int asteroidId, Image line) {
        GameController.AstroidLocation location = GameController.instance.astroidLocations[asteroidId];
        text.text = location.name;

        float alpha = (GameController.instance.farthestAstroid >= asteroidId) ? 1f : 0.5f;
        image.color = new Color(location.color.r * alpha, location.color.g * alpha, location.color.b * alpha, 1);
        text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);

        if (line != null) {
            line.color = new Color(line.color.r, line.color.g, line.color.b, alpha);
        }
	}
}
