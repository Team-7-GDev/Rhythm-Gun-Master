using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameUIController : MonoBehaviour
{

    [SerializeField] private CanvasGroup m_pauseOverlay = null;
    [SerializeField] private CanvasGroup m_gameOverOverlay = null;
    [SerializeField] private CanvasGroup m_settingsOverlay = null;


    [SerializeField] private TMP_Text m_scoreText = null;
    [SerializeField] private TMP_Text m_bulletUsedText = null;
    [SerializeField] private TMP_Text m_headShotText = null;

    [Range(0.125f, 0.75f)]
    [SerializeField] private float m_transitionTime = 0.25f;


    private bool m_isEasing = false;
    private Stack<CanvasGroup> m_overlayStack = null;


    private void OnGamePauseHandler() => ToTransition(m_pauseOverlay);
    private void OnScoreUpdate(int score, int bulletUsed, int headShot)
    {
        m_scoreText.text = score.ToString();
        m_bulletUsedText.text = bulletUsed.ToString();
        m_headShotText.text = headShot.ToString();
    }

    private void Start()
    {
        m_isEasing = false;
        m_overlayStack = new Stack<CanvasGroup>();

        m_pauseOverlay.alpha = 0.0f;
        m_pauseOverlay.gameObject.SetActive(false);

        m_gameOverOverlay.alpha = 0.0f;
        m_gameOverOverlay.gameObject.SetActive(false);

        m_settingsOverlay.alpha = 0.0f;
        m_settingsOverlay.gameObject.SetActive(false);

        m_scoreText.text = "0";
        m_headShotText.text = "0";
        m_bulletUsedText.text = "0";

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
        // m_gameOverOverlay.gameObject.SetActive(true);
        ToTransition(m_gameOverOverlay);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ToTransition(CanvasGroup toGroup)
    {
        if (m_isEasing)
            return;

        m_isEasing = true;

        if (m_overlayStack.Count > 0)
        {
            CanvasGroup currentGroup = m_overlayStack.Peek();
            DOTween.To(() => currentGroup.alpha, (alpha) => currentGroup.alpha = alpha, 0.0f, m_transitionTime).SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                currentGroup.gameObject.SetActive(false);
                toGroup.gameObject.SetActive(true);
                DOTween.To(() => toGroup.alpha, (alpha) => toGroup.alpha = alpha, 1.0f, m_transitionTime).SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    m_isEasing = false;
                    m_overlayStack.Push(toGroup);
                });
            });
        }
        else
        {
            toGroup.gameObject.SetActive(true);
            DOTween.To(() => toGroup.alpha, (alpha) => toGroup.alpha = alpha, 1.0f, m_transitionTime).SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                m_isEasing = false;
                m_overlayStack.Push(toGroup);
            });
        }
    }

    private void PopTransition(Action evt = null)
    {
        if (m_isEasing)
            return;

        m_isEasing = true;
        CanvasGroup currentGroup = m_overlayStack.Pop();
        DOTween.To(() => currentGroup.alpha, (alpha) => currentGroup.alpha = alpha, 0.0f, m_transitionTime).SetEase(Ease.OutSine)
        .OnComplete(() =>
        {
            currentGroup.gameObject.SetActive(false);

            if (m_overlayStack.Count > 0)
            {
                CanvasGroup peekGroup = m_overlayStack.Peek();
                peekGroup.gameObject.SetActive(true);

                DOTween.To(() => peekGroup.alpha, (alpha) => peekGroup.alpha = alpha, 1.0f, m_transitionTime).SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    evt?.Invoke();
                    m_isEasing = false;
                });
            }
            else
            {
                evt?.Invoke();
                m_isEasing = false;
            }
        });
    }



    public void OnSettingsClicked()
    {
        ToTransition(m_settingsOverlay);
    }

    public void OnPopSettingClicked()
    {
        PopTransition();
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
        PopTransition(GameManager.UnPauseGame);
    }
}
