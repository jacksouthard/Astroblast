using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    public static GameController instance;

    public GameObject gameOverScreen;
    public GameObject shipUI;
    public GameObject gameUI;

    void Awake() {
        instance = this;
    }

    void Start() {
        gameOverScreen.SetActive(false);
        shipUI.SetActive(false);
        gameUI.SetActive(false);
    }

    public void ShowShipUI() {
        shipUI.SetActive(true);
        gameUI.SetActive(false);
    }

    public void ShowGameUI() {
        shipUI.SetActive(false);
        gameUI.SetActive(true);
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
