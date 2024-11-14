using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotSpawner : MonoBehaviour
{
    [SerializeField] private BeatPlayer m_BeatPlayer;
    [SerializeField] private GameObject m_Bot;
    [SerializeField] private Transform m_StartPivot;
    [SerializeField] private Transform m_EndPivot;

    private float m_BotTimeout;
    private float m_MinBeatStrength;
    private float m_MaxBeatStrength;

    private List<GameObject> m_BotsPool;


    public void SetDifficulty(DiffcultyType type)
    {
        switch (type)
        {
            case DiffcultyType.EASY:
                m_BotTimeout = 1.0f;
                break;

            case DiffcultyType.MEDIUM:
                m_BotTimeout = 0.7f;
                break;

            case DiffcultyType.HARD:
                m_BotTimeout = 0.55f;
                break;

            case DiffcultyType.EXPERT:
                m_BotTimeout = 0.4f;
                break;
        }
    }


    private void OnEnable()
    {
        m_BeatPlayer.OnBeat += OnBeat;
    }

    private void Start()
    {
        Beat min = new();
        Beat max = new();

        m_BotsPool = new List<GameObject>();

        GetMinMaxBeatWRTStrength(ref min, ref max);
        m_MinBeatStrength = min.strength;
        m_MaxBeatStrength = max.strength;
    }

    private void OnDrawGizmos()
    {
        if (m_StartPivot && m_EndPivot)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(m_StartPivot.position, m_EndPivot.position);
        }
    }

    private void OnDisable()
    {
        m_BeatPlayer.OnBeat -= OnBeat;
    }

    private void OnBeat(Beat[] beats)
    {
        foreach (Beat beat in beats)
        {
            // float t = Mathf.InverseLerp(m_MinBeatStrength, m_MaxBeatStrength, beat.strength); // Don't use beat strength to spawn bot, problem facing it's shifted towards left
            float t = Random.Range(0.0f, 1.0f);
            Vector3 spawnPos = Vector3.Lerp(m_StartPivot.position, m_EndPivot.position, t);

            GameObject bot = GetBotFromPool();
            bot.name = $"Bot ({beat.strength}, {beat.time})";
            bot.transform.position = spawnPos;
            bot.transform.rotation = Quaternion.Euler(0, 180f, 0);

            // Destroy(bot, 0.5f);
            StartCoroutine(DestroyBot(bot, m_BotTimeout));
        }
    }

    private void GetMinMaxBeatWRTStrength(ref Beat min, ref Beat max)
    {
        float minBeatStrength = float.MaxValue;
        float maxBeatStrength = float.MinValue;
        Beat[] beats = m_BeatPlayer.audioData.beats;

        foreach (var beat in beats)
        {
            if (beat.strength < minBeatStrength)
            {
                minBeatStrength = beat.strength;
                min = beat;
            }

            if (beat.strength > maxBeatStrength)
            {
                maxBeatStrength = beat.strength;
                max = beat;
            }
        }
    }

    private GameObject GetBotFromPool()
    {
        for (int i = 0; i < m_BotsPool.Count; i++)
        {
            if (!m_BotsPool[i].activeSelf)
            {
                m_BotsPool[i].SetActive(true);
                return m_BotsPool[i];
            }
        }

        GameObject bot = Instantiate(m_Bot/* , transform */);
        m_BotsPool.Add(bot);

        return bot;
    }

    private IEnumerator DestroyBot(GameObject botGo, float timeout)
    {
        float time = 0;
        do
        {
            yield return null;

            time += Time.deltaTime;
            if (!botGo.activeSelf)
                break;

        } while (time < timeout);

        botGo.SetActive(false);
    }
}
