using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUIController : MonoBehaviour
{

    [SerializeField] private GameObject m_pauseOverlay;
    [SerializeField] private GameObject m_gameOverOverlay;


    [SerializeField] private TMP_Text m_scoreText;
    [SerializeField] private TMP_Text m_bulletUsedText;
    [SerializeField] private TMP_Text m_headShotText;



    private void OnGamePauseHandler() => m_pauseOverlay.SetActive(true);
    private void OnScoreUpdate(int score, int bulletUsed, int headShot)
    {
        m_scoreText.text = score.ToString();
        m_bulletUsedText.text = bulletUsed.ToString();
        m_headShotText.text = headShot.ToString();
    }

    private void Start()
    {
        m_pauseOverlay.SetActive(false);
        m_gameOverOverlay.SetActive(false);

        m_scoreText.text = "0";
        m_bulletUsedText.text = "0";
        m_headShotText.text = "0";

        GameManager.OnGamePause += OnGamePauseHandler;
        GameManager.OnGameScoreUpdate += OnScoreUpdate;
        BeatPlayer.OnPlayCompleted += OnPlayCompletedHandler;
    }

    private void OnDestroy()
    {
        GameManager.OnGamePause -= OnGamePauseHandler;
        GameManager.OnGameScoreUpdate -= OnScoreUpdate;
        BeatPlayer.OnPlayCompleted -= OnPlayCompletedHandler;
    }

    private void OnPlayCompletedHandler()
    {
        m_gameOverOverlay.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnMainMenuClicked()
    {
        GameManager.LoadMainMenu();
    }

    public void OnReplayClicked()
    {
        GameManager.ReplayGame();
    }

    public void OnResumeClicked()
    {
        GameManager.UnPauseGame();
        m_pauseOverlay.SetActive(false);
    }
}
