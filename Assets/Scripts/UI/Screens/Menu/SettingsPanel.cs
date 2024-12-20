using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider m_volumeSlider;
    [SerializeField] private TMP_Text m_volumeValueText;
    [SerializeField] private TMP_Dropdown m_difficultyDropdown;


    private void OnSilderValue(float value) => m_volumeValueText.text = $"{(int)(value * 100)}%";

    private void OnEnable()
    {
        int volumeValue = PlayerPrefs.GetInt("g_volume", 100);
        string difficulty = PlayerPrefs.GetString("g_difficulty", DiffcultyType.EASY.ToString());
        DiffcultyType difficultyEnum = Enum.Parse<DiffcultyType>(difficulty);

        m_volumeValueText.text = $"{volumeValue}%";
        m_volumeSlider.value = volumeValue / 100.0f;
        m_difficultyDropdown.value = (int)difficultyEnum;

        m_volumeSlider.onValueChanged.AddListener(OnSilderValue);
    }

    private void OnDisable()
    {
        DiffcultyType difficultyEnum = (DiffcultyType)m_difficultyDropdown.value;

        PlayerPrefs.SetString("g_difficulty", difficultyEnum.ToString());
        PlayerPrefs.SetInt("g_volume", (int)(m_volumeSlider.value * 100));

        m_volumeSlider.onValueChanged.RemoveListener(OnSilderValue);
    }
}
