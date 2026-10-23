using System;
using UnityEngine;

public static class SettingsData
{
    private const string GameVolumeKey = "volume";   
    private const string UiVolumeKey = "uiVolume";
    private const string SensitivityKey = "sensitivity";

    public static event Action OnChanged;

    public static float GameVolume
    {
        get => PlayerPrefs.GetFloat(GameVolumeKey, 1f);
        set { PlayerPrefs.SetFloat(GameVolumeKey, value); OnChanged?.Invoke(); }
    }

    public static float UiVolume
    {
        get => PlayerPrefs.GetFloat(UiVolumeKey, 0.03f);
        set { PlayerPrefs.SetFloat(UiVolumeKey, value); OnChanged?.Invoke(); }
    }

    public static float Sensitivity
    {
        get => PlayerPrefs.GetFloat(SensitivityKey, 0.1f);
        set { PlayerPrefs.SetFloat(SensitivityKey, value); OnChanged?.Invoke(); }
    }
}