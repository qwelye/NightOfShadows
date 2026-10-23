using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [FormerlySerializedAs("volumeSlider")]
    [SerializeField] private Slider gameVolumeSlider;
    [SerializeField] private Slider uiVolumeSlider;
    [SerializeField] private Slider sensitivitySlider;

    private void Awake()
    {
        gameVolumeSlider.onValueChanged.AddListener(v => SettingsData.GameVolume = v);
        uiVolumeSlider.onValueChanged.AddListener(v => SettingsData.UiVolume = v);
        sensitivitySlider.onValueChanged.AddListener(v => SettingsData.Sensitivity = v);
    }

    private void OnEnable()
    {
        gameVolumeSlider.SetValueWithoutNotify(SettingsData.GameVolume);
        uiVolumeSlider.SetValueWithoutNotify(SettingsData.UiVolume);
        sensitivitySlider.SetValueWithoutNotify(SettingsData.Sensitivity);
    }
}