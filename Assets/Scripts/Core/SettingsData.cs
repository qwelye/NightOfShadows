using System;
using UnityEngine;

public static class SettingsData
{
    private const string VolumeKey = "volume";
    private const string SensitivityKey = "sensitivity";

    public static event Action OnChanged;

    public static float Volume
    {
        get => PlayerPrefs.GetFloat(VolumeKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(VolumeKey, value);
            AudioListener.volume = value;
            OnChanged?.Invoke();
        }
    }

    public static float Sensitivity
    {
        get => PlayerPrefs.GetFloat(SensitivityKey, 0.1f);
        set
        {
            PlayerPrefs.SetFloat(SensitivityKey, value);
            OnChanged?.Invoke();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init() => AudioListener.volume = Volume;
}