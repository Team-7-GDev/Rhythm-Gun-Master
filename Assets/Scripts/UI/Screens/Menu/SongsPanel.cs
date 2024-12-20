using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;


public class SongsPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text m_infoText;
    [SerializeField] private Toggle[] m_toggles;


    string lockvaluesStr = "";

    private void OnEnable()
    {
        StartCoroutine(SetToggleValue());

        int weaponIndex = PlayerPrefs.GetInt("g_weapon", 0);
        lockvaluesStr = PlayerPrefs.GetString("g_weaponStat");

        LockAssets lockValues = JsonUtility.FromJson<LockAssets>(lockvaluesStr);
        WeaponLockValue weaponLockValue = lockValues.lockValues[weaponIndex];

        for (int i = 0; i < weaponLockValue.songIsLock.Count; i++)
        {
            Transform lockIcon = m_toggles[i].transform.Find("LockIcon");
            if (lockIcon != null && weaponLockValue.songIsLock[i] && i < m_toggles.Length)
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

        // 
        m_infoText.gameObject.SetActive(weaponLockValue.songIsLock[1]);
    }

    private void OnDisable()
    {
        for (int i = 0; i < m_toggles.Length; i++)
        {
            if (!m_toggles[i].isOn)
                continue;

            PlayerPrefs.SetInt("g_song", i);
            break;
        }
    }

    private IEnumerator SetToggleValue()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        int toggleIndex = PlayerPrefs.GetInt("g_song", 0);
        m_toggles[toggleIndex].isOn = true;
    }
}
