using UnityEngine;

public class BeatPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioData audioData;
    public AudioSyncer audioSyncer;

    [ReadOnly] public int startIndex;

    private float playbackTime;

    private void Awake()
    {
        audioSource.clip = audioData.clip;
        audioSource.Play();
    }

    private void Update()
    {
        if (!audioSource.isPlaying)
            return;

        playbackTime = audioSource.time;

        bool isBeatDetected = false;
        int beatCount = 0;
        for (int i = startIndex; i < audioData.beats.Length; i++)
        {
            if (playbackTime < audioData.beats[i].time)
            {
                startIndex = i;
                break;
            }

            isBeatDetected = true;
            beatCount++;
        }

        if (isBeatDetected)
        {
            OnBeat(beatCount);
        }
    }

    private void OnDrawGizmos()
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
        Beat next = audioData.beats[startIndex];

        Gizmos.DrawRay(new(playbackTime, 0.0f), new(0.0f, 100.0f * Mathf.Lerp(prev.strength, next.strength, Mathf.InverseLerp(prev.time, next.time, playbackTime))));
    }

    private void OnBeat(int beatCount)
    {
        audioSyncer.OnBeat();
    }
}