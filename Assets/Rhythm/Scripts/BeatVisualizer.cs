using System.Collections.Generic;
using UnityEngine;

public class BeatVisualizer : MonoBehaviour
{
    [SerializeField] private float m_YMultiplier = 1.0f;
    [SerializeField] private BeatGenerator m_Generator;
    [SerializeField] private LineRenderer m_Line;
    [SerializeField] private GameObject m_Bar;

    private List<Vector4> m_Peaks;
    private List<Transform> m_Bars;

    private void OnEnable()
    {
        m_Generator.OnAudioStarted += OnAudioStarted;
        m_Generator.OnAudioCompleted += OnAudioCompleted;
        m_Generator.OnAudioStopped += OnAudioStopped;
    }

    private void Start()
    {
        m_Peaks = new List<Vector4>();
        m_Bars = new List<Transform>();
    }

    private void OnDisable()
    {
        m_Generator.OnAudioStarted -= OnAudioStarted;
        m_Generator.OnAudioCompleted -= OnAudioCompleted;
        m_Generator.OnAudioStopped -= OnAudioStopped;
    }

    private void Update()
    {
        if (!m_Generator || !m_Line)
            return;
        
        List<Vector2> bassData = m_Generator.BassData;

        UpdateLine(bassData);
        UpdatePeaks(bassData);
        UpdateBeats();
    }

    private void OnAudioStarted()
    {
        if (m_Line)
            m_Line.positionCount = 0;

        for (int i = 0; i < m_Bars.Count; i++)
        {
            m_Bars[i].gameObject.SetActive(false);
        }
    }

    private void OnAudioStopped()
    {

    }

    private void OnAudioCompleted()
    {

    }

    private void UpdateLine(List<Vector2> bassData)
    {
        if (m_Generator.IsStopped)
            return;

        int positionCount = m_Line.positionCount;

        if (positionCount < bassData.Count)
        {
            m_Line.positionCount = bassData.Count;

            for (int i = positionCount; i < bassData.Count; i++)
            {
                m_Line.SetPosition(i, new(bassData[i].y, bassData[i].x * m_YMultiplier));
            }
        }
    }

    private void UpdatePeaks(List<Vector2> bassData)
    {
        if (m_Generator.IsStopped)
            return;

        m_Peaks.Clear();
        m_Peaks.Add(new());
        for (int i = 1; i < bassData.Count - 1; i++)
        {
            float prev = bassData[i - 1].x;
            float curr = bassData[i].x;
            float next = bassData[i + 1].x;

            Vector3 pos = m_Line.GetPosition(i);

            if (curr > prev && curr > next)
                m_Peaks.Add(new(curr, pos.x, pos.y, pos.z));

            if (curr < prev && curr < next)
                m_Peaks.Add(new(curr, pos.x, pos.y, pos.z));
        }
    }

    private void UpdateBeats()
    {
        int barIndex = 0;
        for (int i = 1; i < m_Peaks.Count; i++)
        {
            float diff = m_Peaks[i].x - m_Peaks[i - 1].x;
            if (diff <= m_Generator.BeatThreshold)
                continue;

            Vector3 pos = new(m_Peaks[i].y, m_Peaks[i].z + 0.1f, m_Peaks[i].w);

            if (barIndex >= m_Bars.Count)
            {
                GameObject bar = Instantiate(m_Bar, transform);
                bar.name = $"Beat {barIndex + 1}";
                bar.transform.localScale = Vector3.one * 0.15f;

                m_Bars.Add(bar.transform);
            }

            m_Bars[barIndex].gameObject.SetActive(true);
            m_Bars[barIndex].position = pos;

            barIndex++;
        }

        for (int i = barIndex; i < m_Bars.Count; i++)
        {
            m_Bars[i].gameObject.SetActive(false);
        }
    }
}
