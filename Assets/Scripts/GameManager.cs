using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum ChooseWeaponType
{
    PISTOL = 0,
    ASSULT_RIFLE = 1
}

public enum ChooseSongType
{
    BLOODY_MARY = 0,
    ONE_LOVE
}

public enum DiffcultyType
{
    EASY = 0,
    MEDIUM = 1,
    HARD = 2,
    EXPERT = 3
}


public class GameManager : MonoBehaviour
{
    public static Action OnGamePause = null;


    private static GameManager m_Instance = null;
    public static bool IsGameOver { get; private set; } = false;
    public static bool IsGamePause { get; private set; } = false;

    private ChooseSongType m_currentSong;
    private ChooseWeaponType m_currentWeapon;
    private DiffcultyType m_currentDifficulty;

    private BeatPlayer m_beatPlayer = null;
    private Coroutine m_inputCoroutine = null;



    public static void SongChoosen(ChooseSongType type) => m_Instance.m_currentSong = type;
    public static void WeaponChoosen(ChooseWeaponType type) => m_Instance.m_currentWeapon = type;
    public static void DifficultyChoosen(DiffcultyType type) => m_Instance.m_currentDifficulty = type;
    private void OnPlayCompletedHandler() => IsGameOver = true;

    public static void LoadGame()
    {
        SceneManager.LoadScene("RoomA");
    }

    public static void LoadMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public static void ReplayGame()
    {
        SceneManager.LoadScene("RoomA");
    }

    public static void UnPauseGame()
    {
        IsGamePause = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        m_Instance.m_beatPlayer.PauseUnpauseSong(false);
        m_Instance.m_inputCoroutine = m_Instance.StartCoroutine(m_Instance.LookForGamePause());
    }


    private void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoadedHandler;
            BeatPlayer.OnPlayCompleted += OnPlayCompletedHandler;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedHandler;
        BeatPlayer.OnPlayCompleted -= OnPlayCompletedHandler;
    }

    private void OnSceneLoadedHandler(Scene scene, LoadSceneMode loadMode)
    {
        if (scene.name.Equals("Menu"))
        {
            // Do nothing here for now
        }
        else if (scene.name.Equals("RoomA"))
        {
            // setup the game play with the data choosen by the user...
            m_beatPlayer = FindObjectOfType<BeatPlayer>();

            BotSpawner botSpawner = FindObjectOfType<BotSpawner>();
            FirstPersonCameraRotation firstPersonCamera = FindObjectOfType<FirstPersonCameraRotation>();

            // adding song data
            AudioData audioData = Resources.Load<AudioData>(m_currentSong.ToString());
            m_beatPlayer.Initialize(audioData);

            // setting difficulty level
            botSpawner.SetDifficulty(m_currentDifficulty);

            // Adding first person hand
            GameObject go = null;
            switch (m_currentWeapon)
            {
                case ChooseWeaponType.PISTOL:
                    go = Resources.Load<GameObject>("Hands_Gun02");
                    break;

                case ChooseWeaponType.ASSULT_RIFLE:
                    go = Resources.Load<GameObject>("Hands_Automatic_rifle03");
                    break;
            }

            Instantiate<GameObject>(go, Vector3.zero, Quaternion.identity);

            // Initialize camera and beatplayer
            firstPersonCamera.Initialize();

            IsGameOver = false;
            IsGamePause = false;

            m_inputCoroutine = StartCoroutine(LookForGamePause());
        }
    }

    private IEnumerator LookForGamePause()
    {
        do
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !IsGamePause && !IsGameOver)
            {
                IsGamePause = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                m_beatPlayer.PauseUnpauseSong(true);

                StopCoroutine(m_inputCoroutine);

                OnGamePause?.Invoke();
                break;
            }

            yield return null;
        }
        while (m_inputCoroutine != null);

        m_inputCoroutine = null;
    }
}
