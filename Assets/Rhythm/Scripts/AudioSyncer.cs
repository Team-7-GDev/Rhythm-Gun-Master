using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSyncer : MonoBehaviour
{
    public float bias;
    public float timeStep;
    public float timeToBeat;
    public float restSmoothTime;

    private float m_PreviousAudioValue;
    private float m_AudioValue;
    private float m_Timer;

    protected bool m_HasBeated;

    private void Update()
    {
        OnUpdate();
    }

    public virtual void OnUpdate()
    {
        m_PreviousAudioValue = m_AudioValue;
        m_AudioValue = AudioSpectrum.SpectrumValue;

        if (m_PreviousAudioValue > bias && m_AudioValue <= bias)
        {
            if (m_Timer > timeStep)
                OnBeat();
        }

        if (m_PreviousAudioValue <= bias && m_AudioValue > bias)
        {
            if (m_Timer > timeStep)
                OnBeat();
        }

        m_Timer += Time.deltaTime;
    }

    public virtual void OnBeat()
    {
        Debug.Log("Beat");
        m_Timer = 0.0f;
        m_HasBeated = true;
    }
}
