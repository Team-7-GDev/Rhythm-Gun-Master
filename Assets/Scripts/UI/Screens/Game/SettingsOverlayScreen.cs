using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsOverlayScreen : MonoBehaviour
{
    public static Action<float> OnVolumeChange;

    [SerializeField] private Slider m_volumeSlider;
    [SerializeField] private TMP_Text m_volumeValueText;


    private float m_prevVolume;

    private void OnSilderValue(float value) => m_volumeValueText.text = $"{(int)(value * 100)}%";

    private void OnEnable()
    {
        m_prevVolume = PlayerPrefs.GetInt("g_volume", 100);

        m_volumeValueText.text = $"{m_prevVolume}%";
        m_volumeSlider.value = m_prevVolume / 100.0f;

        m_volumeSlider.onValueChanged.AddListener(OnSilderValue);
    }

    private void OnDisable()
    {
        int selectedVolume = (int)(m_volumeSlider.value * 100);
        if (m_prevVolume != selectedVolume)
        {
            PlayerPrefs.SetInt("g_volume", selectedVolume);
            m_volumeSlider.onValueChanged.RemoveListener(OnSilderValue);

            OnVolumeChange?.Invoke(m_volumeSlider.value);
        }
    }
}
