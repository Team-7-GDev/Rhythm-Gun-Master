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
    public static Action<int, int, int> OnGameScoreUpdate = null;


    private static GameManager m_Instance = null;
    public static bool IsGameOver { get; private set; } = false;
    public static bool IsGamePause { get; private set; } = false;

    private ChooseSongType m_currentSong;
    private ChooseWeaponType m_currentWeapon;
    private DiffcultyType m_currentDifficulty;

    private BeatPlayer m_beatPlayer = null;
    private Coroutine m_inputCoroutine = null;


    private int m_bulletUsed = 0;
    private int m_currentScore = 0;
    private int m_totalHeadHit = 0;
    private int m_consecutiveHeadHit = 0;



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

        Time.timeScale = 1.0f;
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
            Time.timeScale = 1.0f;
        }
        else if (scene.name.Equals("RoomA"))
        {
            Time.timeScale = 1.0f;

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
            WeaponsController fpsWeapon = null;
            switch (m_currentWeapon)
            {
                case ChooseWeaponType.PISTOL:
                    fpsWeapon = Resources.Load<WeaponsController>("Hands_Gun02");
                    break;

                case ChooseWeaponType.ASSULT_RIFLE:
                    fpsWeapon = Resources.Load<WeaponsController>("Hands_Automatic_rifle03");
                    break;
            }

            fpsWeapon = Instantiate<WeaponsController>(fpsWeapon, Vector3.zero, Quaternion.identity);
            fpsWeapon.OnWeaponFired = () =>
            {
                m_bulletUsed++;
                OnGameScoreUpdate?.Invoke(m_currentScore, m_bulletUsed, m_totalHeadHit);
            };

            fpsWeapon.OnBotHit = (bool isHead) =>
            {
                m_bulletUsed++;
                if (isHead)
                {
                    m_totalHeadHit++;
                    m_consecutiveHeadHit++;
                    m_currentScore += m_consecutiveHeadHit * 10;
                }
                else
                {
                    m_consecutiveHeadHit = 0;
                    m_currentScore += 5;
                }

                OnGameScoreUpdate?.Invoke(m_currentScore, m_bulletUsed, m_totalHeadHit);
            };

            // Initialize camera and beatplayer
            firstPersonCamera.Initialize();

            IsGameOver = false;
            IsGamePause = false;

            m_bulletUsed = 0;
            m_currentScore = 0;
            m_consecutiveHeadHit = 0;

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

                Time.timeScale = 0.00001f;
                break;
            }

            yield return null;
        }
        while (m_inputCoroutine != null);

        m_inputCoroutine = null;
    }
}
