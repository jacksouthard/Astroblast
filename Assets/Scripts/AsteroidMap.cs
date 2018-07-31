using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AsteroidMap : MonoBehaviour {
    public static AsteroidMap instance;

    public GameObject uiAsteroidPrefab;

    public RectTransform content;
    public ScrollRect scrollArea;

    List<UIAsteroidItem> uiAsteroids;

    void Awake() {
        instance = this;
        scrollArea.enabled = false;
    }

    public void Init() {
        for (int i = 0; i < content.childCount; i++) {
            Destroy(content.GetChild(i).gameObject);
        }

        List<GameController.AstroidLocation> astroids = GameController.instance.astroidLocations;
        for (int i = 0; i < astroids.Count; i++) {
            GameObject newAsteroid = Instantiate(uiAsteroidPrefab, content);
            newAsteroid.GetComponent<RectTransform>().anchoredPosition = new Vector2(astroids[i].xPosition, 100 * (i + 1));
            newAsteroid.GetComponent<UIAsteroidItem>().Init(i);
        }

        content.sizeDelta = new Vector2(0, 100 * (astroids.Count + 1));
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

    public void OnItemSelected(int index) {
        
    }

    public void OnBottomButtonPressed() {
        
    }
}
