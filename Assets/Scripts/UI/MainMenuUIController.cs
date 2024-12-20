using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


// structs to store weapons and song lock values
[Serializable]
public struct WeaponLockValue
{
    public bool weaponIsLock;
    public List<bool> songIsLock;
}

[Serializable]
public struct LockAssets
{
    public List<WeaponLockValue> lockValues;
}


public class MainMenuUIController : MonoBehaviour
{

    [Range(0.125f, 0.75f)]
    [SerializeField] private float m_transitionTime;
    [SerializeField] private VideoPlayer m_videoPlayer;
    [SerializeField] private CanvasGroup m_mainMenuGroup;
    [SerializeField] private CanvasGroup m_settingsMenuGroup;
    [SerializeField] private CanvasGroup m_weaponsMenuGroup;
    [SerializeField] private CanvasGroup m_songsMenuGroup;


    private bool m_isEasing = false;
    private Stack<CanvasGroup> m_menuStack;


    private void Awake() => m_videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "MenuBg.mp4");


    private void Start()
    {
        // GameManager.SongChoosen(ChooseSongType.BLOODY_MARY);
        // GameManager.WeaponChoosen(ChooseWeaponType.PISTOL);
        // GameManager.DifficultyChoosen(DiffcultyType.EASY);

        if (!PlayerPrefs.HasKey("g_weaponStat"))
            PlayerPrefs.SetString("g_weaponStat", "{\"lockValues\":[{\"weaponIsLock\":false,\"songIsLock\":[false,true]},{\"weaponIsLock\":true,\"songIsLock\":[false,true]}]}");

        m_menuStack = new Stack<CanvasGroup>();

        m_mainMenuGroup.gameObject.SetActive(true);

        m_settingsMenuGroup.alpha = 0;
        m_settingsMenuGroup.gameObject.SetActive(false);

        m_weaponsMenuGroup.alpha = 0;
        m_weaponsMenuGroup.gameObject.SetActive(false);

        m_songsMenuGroup.alpha = 0;
        m_songsMenuGroup.gameObject.SetActive(false);

        m_menuStack.Push(m_mainMenuGroup);
    }


    private void ToTransition(CanvasGroup toGroup)
    {
        if (m_isEasing)
            return;

        m_isEasing = true;
        CanvasGroup currentGroup = m_menuStack.Peek();
        DOTween.To(() => currentGroup.alpha, (alpha) => currentGroup.alpha = alpha, 0.0f, m_transitionTime).SetEase(Ease.OutSine)
        .OnComplete(() =>
        {
            currentGroup.gameObject.SetActive(false);
            toGroup.gameObject.SetActive(true);
            DOTween.To(() => toGroup.alpha, (alpha) => toGroup.alpha = alpha, 1.0f, m_transitionTime).SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                m_isEasing = false;
                m_menuStack.Push(toGroup);
            });
        });
    }

    private void PopTransition()
    {
        if (m_isEasing)
            return;

        m_isEasing = true;
        CanvasGroup currentGroup = m_menuStack.Pop();
        DOTween.To(() => currentGroup.alpha, (alpha) => currentGroup.alpha = alpha, 0.0f, m_transitionTime).SetEase(Ease.OutSine)
        .OnComplete(() =>
        {
            CanvasGroup peekGroup = m_menuStack.Peek();

            peekGroup.gameObject.SetActive(true);
            currentGroup.gameObject.SetActive(false);
            DOTween.To(() => peekGroup.alpha, (alpha) => peekGroup.alpha = alpha, 1.0f, m_transitionTime).SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                m_isEasing = false;
            });
        });
    }

    public void OnBackClick() => PopTransition();
    public void OnSettingClick() => ToTransition(m_settingsMenuGroup);
    public void OnStartClick() => ToTransition(m_weaponsMenuGroup);
    public void WeaponsToSongScreen()
    {
        ToTransition(m_songsMenuGroup);
    }

    public void OnPlayClicked()
    {
        SceneManager.LoadSceneAsync("RoomA");
    }

    /* public void OnSongChoosen(Toggle songToggle)
    {
        if (!songToggle.isOn)
            return;

        ChooseSongType type = ChooseSongType.BLOODY_MARY;

        Enum.TryParse<ChooseSongType>(songToggle.name, true, out type);
        GameManager.SongChoosen(type);
    }

    public void OnWeaponChoosen(Toggle weaponToggle)
    {
        if (!weaponToggle.isOn)
            return;

        ChooseWeaponType type = ChooseWeaponType.PISTOL;

        Enum.TryParse<ChooseWeaponType>(weaponToggle.name, true, out type);
        GameManager.WeaponChoosen(type);
    }

    public void OnDifficultyChoosen(Toggle difficultyToggle)
    {
        if (!difficultyToggle.isOn)
            return;

        DiffcultyType type = DiffcultyType.EASY;

        Enum.TryParse<DiffcultyType>(difficultyToggle.name, true, out type);
        GameManager.DifficultyChoosen(type);
    } */
}
