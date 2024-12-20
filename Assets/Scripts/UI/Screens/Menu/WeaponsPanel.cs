using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class WeaponsPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text m_infoText;
    [SerializeField] private Toggle[] m_toggles;

    string lockvalueStr = "";

    private void OnEnable()
    {
        StartCoroutine(SetToggleValue());
        lockvalueStr = PlayerPrefs.GetString("g_weaponStat");

        LockAssets lockValues = JsonUtility.FromJson<LockAssets>(lockvalueStr);
        for (int i = 0; i < lockValues.lockValues.Count; i++)
        {
            Transform lockIcon = m_toggles[i].transform.Find("LockIcon");
            if (lockIcon != null && lockValues.lockValues[i].weaponIsLock && i < m_toggles.Length)
            {
                m_toggles[i].interactable = false;
                lockIcon.gameObject.SetActive(true);
            }
            else if (lockIcon != null)
            {
                m_toggles[i].interactable = true;
                lockIcon.gameObject.SetActive(false);
            }
        }

        // only 2 rifle are present don't complicate the logics
        m_infoText.gameObject.SetActive(lockValues.lockValues[1].weaponIsLock);
    }

    private void OnDisable()
    {
        for (int i = 0; i < m_toggles.Length; i++)
        {
            if (!m_toggles[i].isOn)
                continue;

            PlayerPrefs.SetInt("g_weapon", i);
            break;
        }
    }

    private IEnumerator SetToggleValue()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        int toggleIndex = PlayerPrefs.GetInt("g_weapon", 0);
        m_toggles[toggleIndex].isOn = true;
    }
}
