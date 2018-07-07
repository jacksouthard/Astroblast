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

    public float moneyAddTime;

    public Animator postGameReport;
    public Animator gameOverReport;

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
        StartCoroutine(ShowAddMoney(successMoneyText));
        postGameMoneyText.text = (startingMoney + curSiteMoney).ToString();
        postGameReport.SetTrigger("Open");
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
        StartCoroutine(ShowAddMoney(failMoneyText));
        gameOverReport.SetTrigger("Open");
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

    IEnumerator ShowAddMoney(Text moneyText) {
        yield return new WaitForSeconds(0.5f);

        float p = 0;
        while (p < 1f) {
            moneyText.text = "+"+Mathf.RoundToInt(Mathf.Lerp(0, curSiteMoney, p)).ToString();
            yield return new WaitForEndOfFrame();
            p += Time.deltaTime / moneyAddTime;
        }

        moneyText.text = "+"+curSiteMoney.ToString();
    }
}
