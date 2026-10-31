using System;
using UnityEngine;
public static class SettingsData
{
    private const string GameVolumeKey = "volume";
    private const string UiVolumeKey = "uiVolume";
    private const string SensitivityKey = "sensitivity";

    public static event Action OnChanged;

    private static float gameVolume = -1f;
    private static float uiVolume = -1f;
    private static float sensitivity = -1f;

    public static float GameVolume
    {
        get
        {
            if (gameVolume < 0f) gameVolume = PlayerPrefs.GetFloat(GameVolumeKey, 1f);
            return gameVolume;
        }
        set
        {
            gameVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(GameVolumeKey, gameVolume);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }

    public static float UiVolume
    {
        get
        {
            if (uiVolume < 0f) uiVolume = PlayerPrefs.GetFloat(UiVolumeKey, 0.8f);
            return uiVolume;
        }
        set
        {
            uiVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(UiVolumeKey, uiVolume);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }

    public static float Sensitivity
    {
        get
        {
            if (sensitivity < 0f) sensitivity = PlayerPrefs.GetFloat(SensitivityKey, 0.2f);
            return sensitivity;
        }
        set
        {
            sensitivity = Mathf.Max(0.01f, value);
            PlayerPrefs.SetFloat(SensitivityKey, sensitivity);
            PlayerPrefs.Save();
            OnChanged?.Invoke(); //вызывается событие
        }
    }
}