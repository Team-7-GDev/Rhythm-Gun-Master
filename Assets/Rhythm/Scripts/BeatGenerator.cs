using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BeatGenerator : MonoBehaviour
{
    [SerializeField, Range(0.001f, 0.01f)] private float m_BeatThreshold = 0.0075f;

    private bool m_IsStopped;
    private float m_Percentage;
    private float[] m_Samples;
    private AudioSource m_AudioSource;
    private List<Vector2> m_BassData;
    private List<Vector2> m_PeakData;
    private List<Vector2> m_BeatData;

    public float BeatThreshold
    {
        get => m_BeatThreshold;
        set => m_BeatThreshold = Mathf.Max(0.0f, value);
    }
    public bool IsStopped => m_IsStopped;
    public AudioSource Source => m_AudioSource;
    public List<Vector2> BassData => m_BassData;
    public List<Vector2> PeakData => m_PeakData;
    public List<Vector2> BeatData => m_BeatData;

    public event Action OnAudioStarted;
    public event Action OnAudioStopped;
    public event Action OnAudioCompleted;

    private void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
        m_AudioSource.Stop();

        m_IsStopped = true;
    }

    private void Start()
    {
        m_Samples = new float[8192];
        m_BassData = new List<Vector2>();
        m_PeakData = new List<Vector2>();
        m_BeatData = new List<Vector2>();
    }

    private void Update()
    {
        bool isPlaying = m_AudioSource.isPlaying;

        if (!isPlaying)
        {
            if (!m_IsStopped)
            {
                m_IsStopped = true;
                OnStopped();

                // Audio fully played
                if (m_Percentage > 0.99f)
                {
                    OnComplete();
                }
            }
            return;
        }

        float normalizedProgress = m_AudioSource.time / m_AudioSource.clip.length;
        m_Percentage = Mathf.Clamp01(normalizedProgress);
        m_IsStopped = false;

        GetAudioSamples();

        float bass = GetAverageData(10, 46) / m_AudioSource.volume;

        if (m_BassData.Count == 0 || !Mathf.Approximately(bass, m_BassData[^1].x))
            m_BassData.Add(new(bass, m_AudioSource.time));
    }

    private void GetAudioSamples()
    {
        m_AudioSource.GetSpectrumData(m_Samples, 0, FFTWindow.Blackman);
    }

    private float GetAverageData(int start, int end)
    {
        float sum = 0;
        int count = 0;

        for (int i = start; i <= end; i++)
        {
            sum += m_Samples[i];
            count++;
        }

        return (count == 0) ? 0.0f : sum / count;
    }

    private void OnStopped()
    {
        OnAudioStopped?.Invoke();
    }

    private void OnComplete()
    {
        Debug.Log("Completed");

        GeneratePeakData();
        GenerateBeatData();

        OnAudioCompleted?.Invoke();
    }

    public void GeneratePeakData()
    {
        m_PeakData.Clear();
        m_PeakData.Add(new(0.0f, 0.0f));

        for (int i = 1; i < m_BassData.Count - 1; i++)
        {
            float prev = m_BassData[i - 1].x;
            float curr = m_BassData[i].x;
            float next = m_BassData[i + 1].x;

            if (curr > prev && curr > next)
            {
                m_PeakData.Add(m_BassData[i]);
            }

            if (curr < prev && curr < next)
            {
                m_PeakData.Add(m_BassData[i]);
            }
        }
    }

    public void GenerateBeatData()
    {
        m_BeatData.Clear();

        for (int i = 1; i < m_PeakData.Count; i++)
        {
            float prev = m_PeakData[i - 1].x;
            float curr = m_PeakData[i].x;

            float diff = curr - prev;

            if (diff <= m_BeatThreshold)
                continue;

            m_BeatData.Add(new(m_PeakData[i].x, m_PeakData[i - 1].y));
        }
    }

    public void StartGenerating()
    {
        if (!m_AudioSource.clip || m_AudioSource.isPlaying)
            return;

        m_BassData.Clear();
        m_AudioSource.Play();

        OnAudioStarted?.Invoke();
    }

    public void StopGenerating()
    {
        m_AudioSource.Stop();
    }
}
