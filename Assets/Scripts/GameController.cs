using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    public static GameController instance;

    public GameObject gameOverScreen;

    void Awake() {
        instance = this;
    }

    void Start() {
        gameOverScreen.SetActive(false);
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
