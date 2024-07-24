using System.Collections.Generic;
using UnityEngine;

public class BeatPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioData audioData;

    [ReadOnly] public int startIndex;

    public event System.Action<Beat[]> OnBeat = (_) => { };

    private float playbackTime;
    private List<Beat> m_CurrentBeats;

    private void Awake()
    {
        audioSource.clip = audioData.clip;
        audioSource.Play();
    }

    private void Start()
    {
        m_CurrentBeats = new List<Beat>();
    }

    private void Update()
    {
        if (!audioSource.isPlaying)
            return;

        playbackTime = audioSource.time;

        bool isBeatDetected = false;
        for (int i = startIndex; i < audioData.beats.Length; i++)
        {
            if (playbackTime < audioData.beats[i].time)
            {
                startIndex = i;
                break;
            }

            isBeatDetected = true;
            m_CurrentBeats.Add(audioData.beats[i]);

            if (i == audioData.beats.Length - 1)
                startIndex = i + 1;
        }

        if (isBeatDetected)
        {
            Beat[] beats = m_CurrentBeats.ToArray();
            OnBeat(beats);
            m_CurrentBeats.Clear();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!audioData)
            return;

        Gizmos.color = Color.black;
        foreach (Beat beat in audioData.beats)
        {
            Gizmos.DrawLine(new(beat.time, 0.0f), new(beat.time, beat.strength * 100.0f));
        }

        Gizmos.color = Color.magenta;

        Beat prev = audioData.beats[Mathf.Max(startIndex - 1, 0)];
        Beat next = audioData.beats[Mathf.Min(startIndex, audioData.beats.Length - 1)];

        Gizmos.DrawRay(new(playbackTime, 0.0f), new(0.0f, 100.0f * Mathf.Lerp(prev.strength, next.strength, Mathf.InverseLerp(prev.time, next.time, playbackTime))));
    }
}