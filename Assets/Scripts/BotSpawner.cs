using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: need to implement this with strategy pattern...
public class BotSpawner : MonoBehaviour
{
    [SerializeField] private Bot m_BotPrefab;
    [SerializeField] private BeatPlayer m_BeatPlayer;
    [SerializeField] private Vector3 m_spawnDirection;
    [SerializeField] private float m_spawnOffsetDistance;
    // [SerializeField] private Transform m_StartPivot;
    // [SerializeField] private Transform m_EndPivot;
    [SerializeField] private Transform[] m_spawnPositions;


    private float m_BotTimeout;
    // private float m_MinBeatStrength;
    // private float m_MaxBeatStrength;

    private List<Bot> m_BotsPool;
    // private List<Vector3> m_botPositions;


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

        m_BotsPool = new List<Bot>();

        for (int i = 0; i < m_spawnPositions.Length; i++)
        {
            Bot bot = Instantiate(m_BotPrefab, m_spawnPositions[i].position, Quaternion.Euler(0, 180f, 0));
            m_BotsPool.Add(bot);
        }

        /* m_botPositions = new List<Vector3>(m_spawnPositions.Length);

        for (int i = 0; i < m_spawnPositions.Length; i++)
            m_botPositions.Add(m_spawnPositions[i].position); */

        GetMinMaxBeatWRTStrength(ref min, ref max);
        // m_MinBeatStrength = min.strength;
        // m_MaxBeatStrength = max.strength;
    }

    private void OnDrawGizmos()
    {
        /* if (m_StartPivot && m_EndPivot)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(m_StartPivot.position, m_EndPivot.position);
        } */

        if (m_spawnPositions == null || m_spawnPositions.Length == 0)
            return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < m_spawnPositions.Length; i++)
        {
            Gizmos.DrawWireSphere(m_spawnPositions[i].position, 0.25f);
            Gizmos.DrawRay(m_spawnPositions[i].position + Vector3.up * 0.125f, m_spawnDirection * m_spawnOffsetDistance);
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
            // float t = Random.Range(0.0f, 1.0f);
            // Vector3 spawnPos = Vector3.Lerp(m_StartPivot.position, m_EndPivot.position, t);

            // Bot bot = GetBotFromPool();
            int index = Random.Range(0, m_BotsPool.Count);
            Bot bot = m_BotsPool[index];
            // Vector3 spawnPos = m_botPositions[index] + m_spawnDirection * Random.Range(0, m_spawnOffsetDistance);

            m_BotsPool.RemoveAt(index);
            bot.Initialize(0.25f, AddTobotList);
            // bot.name = $"Bot ({beat.strength}, {beat.time})";
            // bot.transform.SetPositionAndRotation(spawnPos, Quaternion.Euler(0, 180f, 0));

            /* if (m_botPositions.Count == 0)
            {
                for (int i = 0; i < m_spawnPositions.Length; i++)
                    m_botPositions.Add(m_spawnPositions[i].position);
            } */

            // Destroy(bot, 0.5f);
            StartCoroutine(DestroyBot(bot, m_BotTimeout));
        }
    }

    private void AddTobotList(Bot bot)
    {
        m_BotsPool.Add(bot);
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

    /* private Bot GetBotFromPool()
    {
        for (int i = 0; i < m_BotsPool.Count; i++)
        {
            if (!m_BotsPool[i].gameObject.activeSelf)
            {
                m_BotsPool[i].gameObject.SetActive(true);
                return m_BotsPool[i];
            }
        }

        Bot bot = Instantiate(m_Bot);
        m_BotsPool.Add(bot);

        return bot;
    } */

    private IEnumerator DestroyBot(Bot botGo, float timeout)
    {
        float time = 0;
        do
        {
            yield return null;

            time += Time.deltaTime;
            if (!botGo.IsEnable)
                break;

        } while (time < timeout);

        botGo.Hide();
    }
}
