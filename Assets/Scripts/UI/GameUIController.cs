using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIController : MonoBehaviour
{

    [SerializeField] private GameObject m_pauseOverlay;
    [SerializeField] private GameObject m_gameOverOverlay;



    private void OnGamePauseHandler() => m_pauseOverlay.SetActive(true);

    private void Start()
    {
        m_pauseOverlay.SetActive(false);
        m_gameOverOverlay.SetActive(false);

        GameManager.OnGamePause += OnGamePauseHandler;
        BeatPlayer.OnPlayCompleted += OnPlayCompletedHandler;
    }

    private void OnDestroy()
    {
        GameManager.OnGamePause -= OnGamePauseHandler;
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
