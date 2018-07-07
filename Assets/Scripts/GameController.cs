using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    public static GameController instance;


    public GameObject gameOverScreen;
    public GameObject preGameUI;
    public GameObject postGameUI;
    public GameObject gameUI;

    public Text failMoneyText;
    public Text successMoneyText;
    public Text preGameMoneyText;
    public Text postGameMoneyText;

    public MoneyText moneyText;

    int curSiteMoney;
    int startingMoney;

    void Awake() {
        instance = this;
        gameOverScreen.SetActive(false);
        preGameUI.SetActive(false);
        gameUI.SetActive(false);
        postGameUI.SetActive(false);

        startingMoney = PlayerPrefs.GetInt("Total_Money", 0);
        preGameMoneyText.text = startingMoney.ToString();
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
        successMoneyText.text = "+" + curSiteMoney.ToString();
        postGameMoneyText.text = (startingMoney + curSiteMoney).ToString();
        SaveMoney();
    }

    public void OnPlayerDeath() {
        SaveMoney();
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence() {
        yield return new WaitForSeconds(1f); //TODO: link this with the death particle system?
        ShowGameOverUI();
    }

    void ShowGameOverUI() {
        gameOverScreen.SetActive(true);
        failMoneyText.text = "+" + curSiteMoney.ToString();
    }

    public void EndGame() {
        SceneManager.LoadScene(0);
    }

    public void CollectMoney(int newMoney) {
        curSiteMoney += newMoney;
        moneyText.UpdateText(curSiteMoney);
    }

    void SaveMoney() {
        PlayerPrefs.SetInt("Total_Money", startingMoney + curSiteMoney);
    }
}
