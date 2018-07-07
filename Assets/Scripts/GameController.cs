using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    public static GameController instance;

    public GameObject gameOverScreen;
    public GameObject preGameUI;
    public GameObject postGameUI;
    public GameObject gameUI;

    void Awake() {
        instance = this;
        gameOverScreen.SetActive(false);
        preGameUI.SetActive(false);
        gameUI.SetActive(false);
        postGameUI.SetActive(false);
    }

    public void ShowPregame() {
        preGameUI.SetActive(true);
        gameUI.SetActive(false);
        postGameUI.SetActive(false);
    }

    public void ShowGameUI() {
        preGameUI.SetActive(false);
        gameUI.SetActive(true);
        postGameUI.SetActive(false);
    }

    public void HideGameUI() {
        gameUI.SetActive(false);
    }

    public void ShowPostgame() {
        preGameUI.SetActive(false);
        gameUI.SetActive(false);
        postGameUI.SetActive(true);
    }

    public void OnPlayerDeath() {
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence() {
        yield return new WaitForSeconds(1f); //TODO: link this with the death particle system?
        gameOverScreen.SetActive(true);
    }

    public void EndGame() {
        SceneManager.LoadScene(0);
    }
}
