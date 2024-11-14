using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIController : MonoBehaviour
{

    private void Start()
    {
        GameManager.SongChoosen(ChooseSongType.BLOODY_MARY);
        GameManager.WeaponChoosen(ChooseWeaponType.PISTOL);
        GameManager.DifficultyChoosen(DiffcultyType.EASY);
    }

    public void OnSongChoosen(Toggle songToggle)
    {
        if (!songToggle.isOn)
            return;

        ChooseSongType type = ChooseSongType.BLOODY_MARY;

        Enum.TryParse<ChooseSongType>(songToggle.name, true, out type);
        GameManager.SongChoosen(type);
    }

    public void OnWeaponChoosen(Toggle weaponToggle)
    {
        if (!weaponToggle.isOn)
            return;

        ChooseWeaponType type = ChooseWeaponType.PISTOL;

        Enum.TryParse<ChooseWeaponType>(weaponToggle.name, true, out type);
        GameManager.WeaponChoosen(type);
    }

    public void OnDifficultyChoosen(Toggle difficultyToggle)
    {
        if (!difficultyToggle.isOn)
            return;

        DiffcultyType type = DiffcultyType.EASY;

        Enum.TryParse<DiffcultyType>(difficultyToggle.name, true, out type);
        GameManager.DifficultyChoosen(type);
    }

    public void OnStartClick()
    {
        GameManager.LoadGame();
    }
}
