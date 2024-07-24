using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotSpawner : MonoBehaviour
{
    [SerializeField] private BeatPlayer m_BeatPlayer;
    [SerializeField] private GameObject m_Bot;
    [SerializeField] private Transform m_StartPivot;
    [SerializeField] private Transform m_EndPivot;

    private float m_MinBeatStrength;
    private float m_MaxBeatStrength;

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        m_BeatPlayer.OnBeat += OnBeat;
    }

    private void Start()
    {
        Beat min = new();
        Beat max = new();
        GetMinMaxBeatWRTStrength(ref min, ref max);
        m_MinBeatStrength = min.strength;
        m_MaxBeatStrength = max.strength;
    }

    private void Update()
    {
        
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
            float t = Mathf.InverseLerp(m_MinBeatStrength, m_MaxBeatStrength, beat.strength);
            Vector3 spawnPos = Vector3.Lerp(m_StartPivot.position, m_EndPivot.position, t);

            GameObject bot = Instantiate(m_Bot, transform);
            bot.name = $"Bot ({beat.strength}, {beat.time})";
            bot.transform.position = spawnPos;

            Destroy(bot, 0.5f);
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
}
