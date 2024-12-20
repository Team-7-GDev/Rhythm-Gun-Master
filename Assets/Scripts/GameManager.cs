using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum ChooseWeaponType
{
    PISTOL = 0,
    ASSULT_RIFLE = 1
}

public enum ChooseSongType
{
    Song_1 = 0,
    Song_2
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


    [SerializeField] PointText m_pointTextPrefab = null;

    /* private ChooseSongType m_currentSong;
    private ChooseWeaponType m_currentWeapon;
    private DiffcultyType m_currentDifficulty; */

    private BeatPlayer m_beatPlayer = null;
    private Coroutine m_inputCoroutine = null;
    private List<PointText> m_pointTextPool = null;

    private int m_bulletUsed = 0;
    private int m_currentScore = 0;
    private int m_totalHeadHit = 0;
    private int m_consecutiveHeadHit = 0;



    /* public static void SongChoosen(ChooseSongType type) => m_Instance.m_currentSong = type;
    public static void WeaponChoosen(ChooseWeaponType type) => m_Instance.m_currentWeapon = type;
    public static void DifficultyChoosen(DiffcultyType type) => m_Instance.m_currentDifficulty = type; */
    private void OnPlayCompletedHandler()
    {
        IsGameOver = true;

        if (m_currentScore < 2000)
            return;

        // do lock unlock asset values
        // Song 1 = 2000 to unlock 2nd Song . 
        // Song 2 = 4000 to unlock Rifle.
        // Rifle Song 1 - 2000 to Unlock Song 2.
        // Rifle Song 2 - 4000.

        string jsonString;
        int weaponIndex = PlayerPrefs.GetInt("g_weapon", 0);
        string lockValues = PlayerPrefs.GetString("g_weaponStat");
        LockAssets lockAssets = JsonUtility.FromJson<LockAssets>(lockValues);

        switch (weaponIndex)
        {
            case 0: // for gun
                WeaponLockValue gunLockValue = lockAssets.lockValues[0];
                if (gunLockValue.songIsLock[1])
                {
                    // unlock the song...
                    gunLockValue.songIsLock[1] = false;
                    lockAssets.lockValues[0] = gunLockValue;
                }
                else if (m_currentScore >= 4000 && lockAssets.lockValues[1].weaponIsLock)
                {
                    // unlock rifle
                    WeaponLockValue rifleData = lockAssets.lockValues[1];
                    rifleData.weaponIsLock = false;

                    lockAssets.lockValues[1] = rifleData;
                }
                break;

            case 1: // for rifle
                WeaponLockValue rifleLockValue = lockAssets.lockValues[0];
                if (rifleLockValue.songIsLock[1])
                {
                    // unlock the song...
                    rifleLockValue.songIsLock[1] = false;
                    lockAssets.lockValues[0] = rifleLockValue;
                }
                break;
        }

        jsonString = JsonUtility.ToJson(lockAssets);
        PlayerPrefs.SetString("g_weaponStat", jsonString);
    }

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
            m_pointTextPool = new List<PointText>();

            // getting saved values from playerprefs
            string difficulty = PlayerPrefs.GetString("g_difficulty", DiffcultyType.EASY.ToString());
            DiffcultyType currentDifficulty = Enum.Parse<DiffcultyType>(difficulty);

            ChooseSongType currentSong = (ChooseSongType)PlayerPrefs.GetInt("g_song", 0);
            ChooseWeaponType currentWeapon = (ChooseWeaponType)PlayerPrefs.GetInt("g_weapon", 0);


            // setup the game play with the data choosen by the user...
            m_beatPlayer = FindObjectOfType<BeatPlayer>();

            FirstPersonCameraRotation firstPersonCamera = null;
            BotSpawner botSpawner = FindObjectOfType<BotSpawner>();

            // Adding song data
            AudioData audioData = Resources.Load<AudioData>(currentSong.ToString());
            m_beatPlayer.Initialize(audioData);

            // Setting difficulty level
            botSpawner.SetDifficulty(currentDifficulty);

            // Adding first person hand
            WeaponsController fpsWeapon = null;
            switch (currentWeapon)
            {
                case ChooseWeaponType.PISTOL:
                    fpsWeapon = Resources.Load<WeaponsController>("Weapons/Gun_02");
                    break;

                case ChooseWeaponType.ASSULT_RIFLE:
                    fpsWeapon = Resources.Load<WeaponsController>("Weapons/Rifle_03");
                    break;
            }

            fpsWeapon = Instantiate<WeaponsController>(fpsWeapon, Vector3.zero, Quaternion.identity);
            fpsWeapon.OnWeaponFired = () =>
            {
                m_bulletUsed++;
                OnGameScoreUpdate?.Invoke(m_currentScore, m_bulletUsed, m_totalHeadHit);
            };

            fpsWeapon.OnBotHit = (bool isHead, Vector3 hitPos) =>
            {
                PointText pointText = GetPointText();

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

                pointText.ShowPoint(isHead ? 10 : 5, hitPos + Vector3.up * 1.8f);
                OnGameScoreUpdate?.Invoke(m_currentScore, m_bulletUsed, m_totalHeadHit);
            };

            // Initialize camera and beatplayer
            firstPersonCamera = fpsWeapon.GetComponentInChildren<FirstPersonCameraRotation>();
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

    private PointText GetPointText()
    {
        for (int i = 0; i < m_pointTextPool.Count; i++)
        {
            if (!m_pointTextPool[i].gameObject.activeSelf)
                return m_pointTextPool[i];
        }

        PointText pointText = Instantiate<PointText>(m_pointTextPrefab);
        m_pointTextPool.Add(pointText);

        return pointText;
    }
}
