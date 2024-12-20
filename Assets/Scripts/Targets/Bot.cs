using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class Bot : MonoBehaviour
{
    [SerializeField] protected AudioClip m_audioClip;
    [SerializeField] protected AudioSource m_audioSource;
    [SerializeField] protected Collider[] m_colliders;


    private float m_volume = 0f;

    protected Action<Bot> m_callback;

    public bool IsEnable { private set; get; }

    private void OnVolumeChangeHandler(float currVolume) => m_volume = currVolume;

    public virtual void Initialize(float delayTime, Action<Bot> callback)
    {
        IsEnable = true;
        m_callback = callback;

        SettingsOverlayScreen.OnVolumeChange += OnVolumeChangeHandler;
    }

    public virtual void Hide()
    {
        IsEnable = false;
        m_callback?.Invoke(this);

        SettingsOverlayScreen.OnVolumeChange -= OnVolumeChangeHandler;
    }

    public virtual void OnHit(Vector3 onPos)
    {
        Hide();
        m_audioSource.PlayOneShot(m_audioClip, m_volume);
    }
}
