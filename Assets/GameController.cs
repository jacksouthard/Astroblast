using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    public static GameController instance;

    void Awake() {
        instance = this;
    }

    public void OnPlayerDeath() {
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence() {
        yield return new WaitForSeconds(1f); //TODO: link this with the death particle system?
        EndGame();
    }

    void EndGame() {
        SceneManager.LoadScene(0);
    }
}
