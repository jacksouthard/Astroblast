using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AsteroidMap : MonoBehaviour {
    public static AsteroidMap instance;

    public GameObject uiLinePrefab;
    public GameObject uiAsteroidPrefab;

    public RectTransform content;
    public ScrollRect scrollArea;

    void Awake() {
        instance = this;
        scrollArea.enabled = false;
    }

    public void Init() {
        ClearChild(content.GetChild(0));
        ClearChild(content.GetChild(1));

        List<GameController.AstroidLocation> astroids = GameController.instance.astroidLocations;
        Vector2 lastPos = Vector2.zero;
        Vector2 curPos = Vector2.zero;

        for (int i = 0; i < astroids.Count; i++) {
            GameObject newAsteroid = Instantiate(uiAsteroidPrefab, content.GetChild(1));
            curPos = new Vector2(astroids[i].xPosition, 150 * (i + 1) + 50);

            newAsteroid.GetComponent<RectTransform>().anchoredPosition = curPos;

            Image lineImage = null;
            if (i > 0) {
                GameObject newLine = Instantiate(uiLinePrefab, content.GetChild(0));
                Vector2 diff = curPos - lastPos;
                float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

                RectTransform lineRect = newLine.GetComponent<RectTransform>();
                lineRect.anchoredPosition = lastPos;
                lineRect.rotation = Quaternion.Euler(0, 0, angle);
                lineRect.sizeDelta = new Vector2(diff.magnitude, lineRect.sizeDelta.y);

                lineImage = newLine.GetComponent<Image>();
            }

            newAsteroid.GetComponent<UIAsteroidItem>().Init(i, lineImage);

            lastPos = curPos;
        }

        content.sizeDelta = new Vector2(0, 100 * (astroids.Count + 1));
    }

    void ClearChild(Transform parent) {
        for (int i = 0; i < parent.childCount; i++) {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public void OnOpen() {
        StartCoroutine(OpenSequence());
    }

    IEnumerator OpenSequence() {
        yield return new WaitForSeconds(0.5f);
        scrollArea.enabled = true;
        content.anchoredPosition = Vector2.zero;

		MessageManager.instance.OnMapOpened ();
    }

    public void OnClose() {
        scrollArea.enabled = false;
    }

    public void OnItemSelected(int index) {
        
    }

    public void OnBottomButtonPressed() {
        
    }
}
