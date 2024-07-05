using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSpectrum : MonoBehaviour
{
    private AudioSource m_AudioSource;
    private float[] m_AudioSamples;

    public static float SpectrumValue { get; private set; }

    private void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        const int audioSampleCount = 128;
        m_AudioSamples = new float[audioSampleCount];
    }

    private void Update()
    {
        AudioListener.GetSpectrumData(m_AudioSamples, 0, FFTWindow.Hamming);

        if (m_AudioSamples != null && m_AudioSamples.Length > 0)
            SpectrumValue = m_AudioSamples[0] * 100.0f;
    }
}
